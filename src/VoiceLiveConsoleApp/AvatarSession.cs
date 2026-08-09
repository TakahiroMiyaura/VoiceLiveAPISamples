// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars.Streaming;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;
using Microsoft.Extensions.Logging;
using AvatarMessageHandlerManager = Com.Reseul.Azure.AI.VoiceLiveAPI.Core.AvatarMessageHandlerManager;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{
    /// <summary>
    ///     The avatar half of a session: what the avatar is, how its video reaches the screen, and the
    ///     lifetime of the pieces that carry it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Two transports, chosen in the menu. <b>WebRTC</b> negotiates an SDP/ICE peer connection and
    ///         carries both video and audio on the media stream. <b>WebSocket</b> sets
    ///         <c>output_protocol=websocket</c>, so frames arrive as <c>response.video.delta</c> on the session
    ///         socket with no negotiation at all, and audio stays on the standard PCM path.
    ///     </para>
    ///     <para>
    ///         Two kinds of avatar, independent of the transport. A <b>video avatar</b> is a pre-rendered
    ///         character that has to be cropped out of a wider frame; a <b>photo avatar</b> is generated from a
    ///         single portrait by <c>vasa-1</c> and is already a head shot. What differs between them is
    ///         explained on <see cref="BuildConfiguration" />.
    ///     </para>
    /// </remarks>
    public sealed class AvatarSession : IDisposable
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The standard photo avatar characters ("Talking heads"). Used only to tell a standard character
        ///     from a custom one, so an unknown name is sent as <c>customized</c> instead of being rejected:
        ///     a custom name sent without that flag is looked up among the standard characters, and the
        ///     service reports the miss with an error whose fields are all null.
        /// </summary>
        private static readonly HashSet<string> StandardTalkingHeads = new HashSet<string>
        {
            "adrian", "amara", "amira", "anika", "bianca", "camila", "carlos", "clara", "darius", "diego",
            "elise", "farhan", "faris", "gabrielle", "hyejin", "imran", "isabella", "layla", "liwei", "ling",
            "marcus", "matteo", "rahul", "rana", "ren", "riya", "sakura", "simone", "zayd", "zoe"
        };

        #endregion

        #region Private Fields

        /// <summary>The logger, or null to stay quiet.</summary>
        private ILogger? logger;

        /// <summary>Handles avatar-specific server messages (the SDP answer).</summary>
        private AvatarMessageHandlerManager? messageManager;

        /// <summary>The WebRTC peer connection carrying the avatar's media.</summary>
        private AvatarClient? client;

        /// <summary>Renders the WebRTC media stream.</summary>
        private AvatarVideoStreamer? videoStreamer;

        /// <summary>Renders frames that arrive as <c>response.video.delta</c>.</summary>
        private WebSocketAvatarVideoStreamer? webSocketStreamer;

        #endregion

        #region Properties

        /// <summary>
        ///     Sets the logger. The session is constructed before logging is configured, so this is handed
        ///     over once it is.
        /// </summary>
        public ILogger? Logger
        {
            set => logger = value;
        }

        /// <summary>Gets or sets a value indicating whether an avatar is part of this session at all.</summary>
        public bool IsEnabled { get; set; }

        /// <summary>Gets or sets what drives the session underneath the avatar: <c>agent</c> or <c>model</c>.</summary>
        public string Backend { get; set; } = "agent";

        /// <summary>Gets or sets a value indicating whether video arrives over the WebSocket.</summary>
        public bool UseWebSocketVideo { get; set; }

        /// <summary>Gets or sets a value indicating whether the avatar is a photo avatar.</summary>
        public bool UsePhoto { get; set; }

        /// <summary>Gets a value indicating whether the session runs on a Foundry agent.</summary>
        public bool IsAgentBacked =>
            !string.Equals(Backend, "model", StringComparison.OrdinalIgnoreCase);

        /// <summary>Gets the media transport to request in the avatar configuration.</summary>
        public string OutputProtocol => UseWebSocketVideo
            ? Avatar.OutputProtocols.WebSocket
            : Avatar.OutputProtocols.WebRtc;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AvatarSession" /> class.
        /// </summary>
        /// <param name="logger">The logger, or null.</param>
        public AvatarSession(ILogger? logger)
        {
            this.logger = logger;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Forgets the previous run's choices. Called when the mode is picked, so switching away from the
        ///     avatar and back does not inherit stale settings.
        /// </summary>
        public void Reset()
        {
            IsEnabled = false;
            UseWebSocketVideo = false;
            UsePhoto = false;
        }

        /// <summary>
        ///     Builds the session's <c>avatar</c> object for whichever kind of avatar was chosen.
        /// </summary>
        /// <remarks>
        ///     The two kinds need different fields. A video avatar is a full-body character, so it is cropped
        ///     to the speaker and given a style. A photo avatar needs its base model (<c>vasa-1</c>), takes no
        ///     style at all — every talking head has none — and needs no crop, being a head shot already. Its
        ///     resolution is not honored either: frames arrive at the source portrait's aspect ratio.
        /// </remarks>
        /// <returns>The avatar configuration.</returns>
        public Avatar BuildConfiguration()
        {
            return UsePhoto ? BuildPhotoAvatar() : BuildVideoAvatar();
        }

        /// <summary>
        ///     Creates the pieces the chosen transport needs and registers the avatar message handler.
        /// </summary>
        /// <param name="session">The live session.</param>
        public void Attach(VoiceLiveSession session)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (UseWebSocketVideo)
            {
                // Frames arrive as response.video.delta on this same socket: no WebRTC, no SDP, no ICE.
                webSocketStreamer = new WebSocketAvatarVideoStreamer(
                    logger ?? throw new InvalidOperationException("logger is required for avatar video"));

                if (!webSocketStreamer.Start())
                {
                    logger?.LogError("Failed to start WebSocket avatar video streamer (is ffplay in PATH?)");
                }

                logger?.LogInformation("Avatar mode (WebSocket): video will stream via response.video.delta");
                return;
            }

            messageManager = new AvatarMessageHandlerManager();
            messageManager.OnSessionAvatarConnecting += connecting =>
            {
                logger?.LogDebug("type : {Type}", connecting.Type);

                if (client == null)
                {
                    logger?.LogError("Avatar connecting event received but the avatar client is null");
                    return;
                }

                logger?.LogTrace("Setting remote SDP for WebRTC connection");
                client.AvatarConnecting(connecting.ServerSdp);
                logger?.LogTrace("Remote SDP set successfully");
            };

            session.AddMessageHandlerManager(messageManager);
            client = new AvatarClient(logger);
            logger?.LogInformation("Avatar client initialized for WebRTC streaming");
        }

        /// <summary>
        ///     Renders one frame delivered over the WebSocket.
        /// </summary>
        /// <param name="frame">The frame bytes (fragmented MP4).</param>
        public void WriteVideoFrame(byte[] frame)
        {
            webSocketStreamer?.WriteFrame(frame);
        }

        /// <summary>
        ///     Negotiates the WebRTC connection once the service has reported its ICE servers, then starts
        ///     rendering. Does nothing for the WebSocket transport, which needs no negotiation.
        /// </summary>
        /// <param name="iceServers">The ICE servers from <c>session.updated</c>.</param>
        /// <param name="session">The live session, used to send the offer.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ConnectWebRtcAsync(IceServers[]? iceServers, VoiceLiveSession session)
        {
            if (!IsEnabled || UseWebSocketVideo || client == null)
            {
                return;
            }

            if (iceServers == null || iceServers.Length == 0)
            {
                logger?.LogWarning("Avatar is set but IceServers is null or empty");
                return;
            }

            try
            {
                IceServers ice = iceServers[0];
                logger?.LogInformation("Starting WebRTC connection with ICE servers: {urls}",
                    string.Join(", ", ice.Urls));

                // Build a neutral ICE config, create the SDP offer (media plane via AvatarClient), and send it
                // through the self-made Core signaling (session.avatar.connect).
                var avatarIce = new AvatarIceServer
                {
                    Urls = ice.Urls,
                    UserName = ice.UserName,
                    Credential = ice.Credential
                };

                string clientSdp = await client.CreateSdpOfferStringAsync(avatarIce);
                await new SessionAvatarConnect { ClientSdp = clientSdp }.SendAsync(session);

                logger?.LogInformation("WebRTC connection initiated successfully");

                StartVideoStreaming();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to establish the avatar WebRTC connection");
                StopVideoStreaming();
            }
        }

        /// <summary>
        ///     Stops rendering and releases the transport. Safe to call repeatedly, which is why teardown,
        ///     mode switching and reconnection can all share it.
        /// </summary>
        public void Dispose()
        {
            StopVideoStreaming();

            if (webSocketStreamer != null)
            {
                webSocketStreamer.Dispose();
                webSocketStreamer = null;
            }

            // AvatarClient holds no unmanaged handle of its own — the peer connection goes with the
            // streamer released above — so dropping the reference is the whole teardown.
            client = null;

            messageManager = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Starts rendering the WebRTC media stream.
        /// </summary>
        private void StartVideoStreaming()
        {
            if (videoStreamer != null || client == null)
            {
                return;
            }

            videoStreamer = new AvatarVideoStreamer(client,
                logger ?? throw new InvalidOperationException("logger is required for avatar video"));

            if (!videoStreamer.StartStreaming())
            {
                logger?.LogError("Failed to start avatar video streaming");
                StopVideoStreaming();
            }
        }

        /// <summary>
        ///     Stops rendering the WebRTC media stream.
        /// </summary>
        private void StopVideoStreaming()
        {
            if (videoStreamer == null)
            {
                return;
            }

            videoStreamer.StopStreaming();
            videoStreamer.Dispose();
            videoStreamer = null;
        }

        /// <summary>
        ///     Builds the configuration for the standard video avatar: a pre-rendered, full-body character,
        ///     cropped to the speaker so the frame isn't mostly empty background.
        /// </summary>
        /// <returns>The avatar configuration.</returns>
        private Avatar BuildVideoAvatar()
        {
            return new Avatar
            {
                Type = Avatar.Types.VideoAvatar,
                Character = "lisa",
                Style = "casual-sitting",
                Customized = false,
                OutputProtocol = OutputProtocol,
                Video = new Video
                {
                    BitRate = 2000000,
                    Codec = "h264",
                    Crop = new Crop
                    {
                        TopLeft = new[] { 560, 0 },
                        BottomRight = new[] { 1360, 1080 }
                    },
                    Resolution = new Resolution
                    {
                        Width = 1920,
                        Height = 1080
                    },
                    Background = new Background
                    {
                        Color = "#FFFFFFFF"
                    }
                }
            };
        }

        /// <summary>
        ///     Builds the configuration for a photo avatar: a single portrait animated by the vasa-1 base
        ///     model.
        /// </summary>
        /// <returns>The avatar configuration.</returns>
        private Avatar BuildPhotoAvatar()
        {
            string character = ConsoleSettings.GetOr("PhotoAvatarCharacter", "sakura");

            bool forced = ConsoleSettings.GetFlag("PhotoAvatarCustomized");
            bool isStandard = StandardTalkingHeads.Contains(character.Trim().ToLowerInvariant());
            bool customized = forced || !isStandard;

            // Say why, not just what: forcing the flag on and then naming a standard character sends it to the
            // custom namespace, where it does not exist — and the service reports that with an empty error.
            string reason = !customized ? "standard talking head"
                : forced && isStandard ? "custom — forced by PhotoAvatarCustomized, though it names a standard character"
                : forced ? "custom — forced by PhotoAvatarCustomized"
                : "custom — not a standard talking head";

            Console.WriteLine($"Photo avatar: character '{character}' ({reason})");

            return new Avatar
            {
                Type = Avatar.Types.PhotoAvatar,
                Model = Avatar.PhotoBaseModes.Vasa1,
                Character = character,
                Customized = customized,
                OutputProtocol = OutputProtocol,
                Video = new Video
                {
                    BitRate = 2000000,
                    Codec = "h264",
                    Resolution = new Resolution
                    {
                        Width = 1920,
                        Height = 1080
                    },
                    Background = new Background
                    {
                        Color = "#FFFFFFFF"
                    }
                }
            };
        }

        #endregion
    }
}
