// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveSDK
{
    /// <summary>
    ///     Handles avatar WebRTC video streaming integration with the Azure.AI.VoiceLive SDK.
    /// </summary>
    internal class AvatarHandler : IDisposable
    {
        #region Private Fields

        private readonly ILogger logger;

        private AvatarClient? avatarClient;
        private AvatarVideoStreamer? avatarVideoStreamer;
        private bool disposed;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a value indicating whether the avatar client is initialized.
        /// </summary>
        public bool IsInitialized => avatarClient != null;

        /// <summary>
        ///     Gets a value indicating whether avatar video streaming is active.
        /// </summary>
        public bool IsStreaming => avatarVideoStreamer != null;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AvatarHandler" /> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public AvatarHandler(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Initializes the avatar client for WebRTC streaming.
        /// </summary>
        public void Initialize()
        {
            avatarClient = new AvatarClient();
            logger.LogInformation("Avatar client initialized for WebRTC streaming");
        }

        /// <summary>
        ///     Creates a Base64-encoded SDP offer for the SDK's ConnectAvatarAsync.
        /// </summary>
        /// <param name="iceServers">The ICE server information from session.updated.</param>
        /// <returns>A Base64-encoded SDP offer string.</returns>
        public async Task<string> CreateSdpOfferAsync(IceServers iceServers)
        {
            if (avatarClient == null)
            {
                throw new InvalidOperationException("Avatar client not initialized. Call Initialize() first.");
            }

            logger.LogInformation("Creating SDP offer with ICE servers: {urls}",
                string.Join(", ", iceServers.Urls));

            string sdp = await avatarClient.CreateSdpOfferStringAsync(iceServers);

            logger.LogInformation("SDP offer created successfully");
            return sdp;
        }

        /// <summary>
        ///     Processes the server SDP answer from session.avatar.connecting event.
        /// </summary>
        /// <param name="serverSdp">The server SDP answer string.</param>
        public void ProcessServerSdpAnswer(string serverSdp)
        {
            if (avatarClient == null)
            {
                logger.LogError("Avatar connecting event received but avatarClient is null");
                return;
            }

            logger.LogTrace("Setting remote SDP for WebRTC connection");
            avatarClient.AvatarConnecting(serverSdp);
            logger.LogTrace("Remote SDP set successfully");
        }

        /// <summary>
        ///     Starts avatar video streaming with FFmpeg integration.
        /// </summary>
        /// <returns>True if streaming started successfully.</returns>
        public bool StartVideoStreaming()
        {
            if (avatarClient == null)
            {
                logger.LogError("Cannot start video streaming: avatar client not initialized");
                return false;
            }

            if (avatarVideoStreamer != null)
            {
                logger.LogInformation("Avatar video streaming is already active");
                return true;
            }

            avatarVideoStreamer = new AvatarVideoStreamer(avatarClient, logger);

            if (!avatarVideoStreamer.StartStreaming())
            {
                logger.LogError("Failed to start avatar video streaming");
                avatarVideoStreamer.Dispose();
                avatarVideoStreamer = null;
                return false;
            }

            logger.LogInformation("Avatar video streaming started");
            return true;
        }

        /// <summary>
        ///     Shows information about the current avatar streaming state.
        /// </summary>
        public void ShowStreamingInfo()
        {
            if (avatarVideoStreamer == null)
            {
                Console.WriteLine("Avatar video streamer not initialized. Connect to avatar first.");
                return;
            }

            Console.WriteLine("Avatar streaming information:");
            Console.WriteLine("   - Real-time RTP streaming is active");
            Console.WriteLine("   - Video window opens automatically when streaming starts");
            Console.WriteLine("   - All playback is real-time only");
        }

        /// <summary>
        ///     Toggles avatar video streaming display information.
        /// </summary>
        public void ToggleVideoStreaming()
        {
            if (avatarVideoStreamer == null)
            {
                Console.WriteLine("Avatar video streamer not initialized. Connect to avatar first.");
                return;
            }

            Console.WriteLine("Avatar RTP streaming is active");
            Console.WriteLine("   - Status: Real-time synchronized audio/video playback");
            Console.WriteLine("   - Video window opens automatically when streaming starts");
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        ///     Releases resources used by the avatar handler.
        /// </summary>
        public void Dispose()
        {
            if (disposed) return;

            if (avatarVideoStreamer != null)
            {
                avatarVideoStreamer.StopStreaming();
                avatarVideoStreamer.Dispose();
                avatarVideoStreamer = null;
            }

            avatarClient = null;
            disposed = true;
        }

        #endregion
    }
}
