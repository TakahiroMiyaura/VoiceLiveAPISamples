// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text;
using System.Text.Json;
using Azure.AI.VoiceLive;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveSDK
{
    /// <summary>
    ///     Manages VoiceLive SDK session lifecycle and event processing.
    /// </summary>
    internal class VoiceLiveAssistant : IAsyncDisposable
    {
        #region Private Fields

        private readonly VoiceLiveClient client;
        private readonly AudioHandler audioHandler;
        private readonly AvatarHandler? avatarHandler;
        private readonly ConnectionMode mode;
        private readonly ILogger logger;

        private VoiceLiveSession? session;
        private CancellationTokenSource? eventProcessingCts;
        private Task? eventProcessingTask;
        private bool disposed;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a value indicating whether the session is connected.
        /// </summary>
        public bool IsConnected => session != null;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveAssistant" /> class.
        /// </summary>
        /// <param name="client">The VoiceLive SDK client.</param>
        /// <param name="audioHandler">The audio handler for input/output.</param>
        /// <param name="avatarHandler">The avatar handler (null if not in Avatar mode).</param>
        /// <param name="mode">The connection mode.</param>
        /// <param name="logger">The logger instance.</param>
        public VoiceLiveAssistant(
            VoiceLiveClient client,
            AudioHandler audioHandler,
            AvatarHandler? avatarHandler,
            ConnectionMode mode,
            ILogger logger)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.audioHandler = audioHandler ?? throw new ArgumentNullException(nameof(audioHandler));
            this.avatarHandler = avatarHandler;
            this.mode = mode;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Starts a new VoiceLive session for the specified target (an AI model or a Foundry agent).
        /// </summary>
        /// <param name="target">The session target: a model name or an agent configuration.</param>
        /// <param name="sessionOptions">The session options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task StartAsync(
            SessionTarget target,
            VoiceLiveSessionOptions sessionOptions,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Starting VoiceLive SDK session in {mode} mode...", mode);

            // AI Model uses a model session; AI Agent / Avatar use a Foundry agent session
            // (AgentSessionConfig). SessionTarget unifies both via the SDK's StartSessionAsync overload.
            session = await client.StartSessionAsync(target, cancellationToken).ConfigureAwait(false);

            logger.LogInformation("VoiceLive SDK session started");

            // Configure session
            await session.ConfigureSessionAsync(sessionOptions, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Session configured");

            // Wire up audio input
            audioHandler.OnAudioDataAvailable += OnMicrophoneAudioAvailable;

            // Start event processing loop
            eventProcessingCts = new CancellationTokenSource();
            eventProcessingTask = ProcessEventsAsync(eventProcessingCts.Token);

            logger.LogInformation("Event processing started");
        }

        /// <summary>
        ///     Sends a response cancel request.
        /// </summary>
        public async Task CancelResponseAsync(CancellationToken cancellationToken = default)
        {
            if (session == null) return;
            await session.CancelResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Clears the streaming audio buffer on the server.
        /// </summary>
        public async Task ClearStreamingAudioAsync(CancellationToken cancellationToken = default)
        {
            if (session == null) return;
            await session.ClearStreamingAudioAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Sends a response creation request.
        /// </summary>
        public async Task StartResponseAsync(CancellationToken cancellationToken = default)
        {
            if (session == null) return;
            await session.StartResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Sends an image as a user message and requests a response. The image is added via a raw
        ///     <c>conversation.item.create</c> event with an <c>input_image</c> content part (the
        ///     strongly-typed <see cref="UserMessageItem" /> does not expose image content in the
        ///     current SDK). The model must be vision-capable; in Avatar mode the spoken description is
        ///     rendered by the avatar. The image field name is <c>image_url</c> (required by the live service).
        /// </summary>
        /// <param name="imagePath">Path to a local image file (png/jpg/gif/webp).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task SendImageAsync(string imagePath, CancellationToken cancellationToken = default)
        {
            if (session == null)
            {
                logger.LogWarning("Cannot send image: session is not started.");
                return;
            }

            // Pause microphone input so the server VAD does not race with (or already hold an active
            // response for) the image turn — otherwise the service can reject the item and close.
            if (audioHandler.IsRecording)
            {
                audioHandler.StopRecording();
                logger.LogInformation("Recording paused for image input.");
            }

            byte[] bytes = File.ReadAllBytes(imagePath);
            string mime = Path.GetExtension(imagePath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/png"
            };
            string dataUri = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";

            var itemCreate = new
            {
                type = "conversation.item.create",
                item = new
                {
                    type = "message",
                    role = "user",
                    content = new object[]
                    {
                        new { type = "input_text", text = "この画像に何が写っているか説明してください。" },
                        new { type = "input_image", image_url = dataUri }
                    }
                }
            };

            await session.SendCommandAsync(BinaryData.FromString(JsonSerializer.Serialize(itemCreate)), cancellationToken)
                .ConfigureAwait(false);
            await session.StartResponseAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Image sent ({bytes} bytes); response requested.", bytes.Length);
        }

        #endregion

        #region Private Methods

        private async void OnMicrophoneAudioAvailable(byte[] audioData)
        {
            if (session == null) return;

            try
            {
                await session.SendInputAudioAsync(audioData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError("Error sending audio data: {Message}", ex.Message);
            }
        }

        private async Task ProcessEventsAsync(CancellationToken cancellationToken)
        {
            if (session == null) return;

            try
            {
                await foreach (SessionUpdate update in session.GetUpdatesAsync(cancellationToken))
                {
                    try
                    {
                        await HandleSessionUpdateAsync(update).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error handling session update: {type}", update.GetType().Name);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Event processing loop error");
            }

            logger.LogInformation("Event processing ended");
        }

        private async Task HandleSessionUpdateAsync(SessionUpdate update)
        {
            switch (update)
            {
                case SessionUpdateSessionCreated sessionCreated:
                    logger.LogInformation("Session created");
                    break;

                case SessionUpdateSessionUpdated sessionUpdated:
                    logger.LogInformation("Session updated");
                    await HandleSessionUpdatedAsync(sessionUpdated).ConfigureAwait(false);
                    break;

                case SessionUpdateResponseAudioDelta audioDelta:
                    HandleAudioDelta(audioDelta);
                    break;

                case SessionUpdateResponseAudioTranscriptDelta transcriptDelta:
                    logger.LogTrace("Transcript delta: {delta}", transcriptDelta.Delta);
                    break;

                case SessionUpdateConversationItemInputAudioTranscriptionCompleted transcription:
                    logger.LogTrace("Transcription: {transcript}", transcription.Transcript);
                    break;

                case SessionUpdateInputAudioBufferSpeechStarted speechStarted:
                    logger.LogTrace("Speech started");
                    break;

                case SessionUpdateInputAudioBufferSpeechStopped speechStopped:
                    logger.LogTrace("Speech stopped (audio_end: {ms}ms)", speechStopped.AudioEnd);
                    if (audioHandler.IsRecording)
                    {
                        audioHandler.StopRecording();
                    }
                    break;

                case SessionUpdateResponseDone responseDone:
                    logger.LogTrace("Response done: {response}", responseDone.Response);
                    break;

                case SessionUpdateResponseOutputItemDone outputItemDone:
                    logger.LogTrace("Output item done");
                    break;

                case SessionUpdateConversationItemCreated itemCreated:
                    logger.LogTrace("Conversation item created");
                    break;

                case SessionUpdateAvatarConnecting avatarConnecting:
                    logger.LogInformation("Avatar connecting - processing server SDP answer");
                    HandleAvatarConnecting(avatarConnecting);
                    break;

                case SessionUpdateError error:
                    logger.LogError("Server error: {code} - {message}", error.Error.Code, error.Error.Message);
                    Console.WriteLine("[Error] {0}: {1}", error.Error.Code, error.Error.Message);
                    break;

                default:
                    logger.LogTrace("Received update: {type}", update.GetType().Name);
                    break;
            }
        }

        private async Task HandleSessionUpdatedAsync(SessionUpdateSessionUpdated sessionUpdated)
        {
            if (avatarHandler == null || mode != ConnectionMode.Avatar || session == null)
            {
                // Non-avatar mode: just start recording
                audioHandler.StartRecording();
                return;
            }

            // Avatar mode: extract ICE servers and initiate WebRTC connection
            try
            {
                logger.LogInformation("Avatar mode: Checking for ICE servers in session update...");

                // Extract ICE servers from SDK properties
                AvatarIceServer? iceServers = ExtractIceServersFromUpdate(sessionUpdated);

                if (iceServers == null)
                {
                    logger.LogWarning("No ICE servers found in session update");
                    audioHandler.StartRecording();
                    return;
                }

                // Create SDP offer using AvatarClient
                string sdpOffer = await avatarHandler.CreateSdpOfferAsync(iceServers);

                // Send SDP offer via SDK session
                // The server will respond with SessionUpdateAvatarConnecting containing the SDP answer
                await session.ConnectAvatarAsync(sdpOffer).ConfigureAwait(false);
                logger.LogInformation("Avatar SDP offer sent via SDK ConnectAvatarAsync");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling avatar session update");
            }

            audioHandler.StartRecording();
        }

        private AvatarIceServer? ExtractIceServersFromUpdate(SessionUpdateSessionUpdated sessionUpdated)
        {
            try
            {
                var sdkIceServers = sessionUpdated.Session?.Avatar?.IceServers;
                if (sdkIceServers == null || sdkIceServers.Count == 0)
                {
                    return null;
                }

                var firstServer = sdkIceServers[0];
                var urls = firstServer.Uris.Select(u => u.ToString()).ToArray();

                logger.LogInformation("ICE servers found: {urls}", string.Join(", ", urls));

                return new AvatarIceServer
                {
                    Urls = urls,
                    UserName = firstServer.Username,
                    Credential = firstServer.Credential
                };
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not extract ICE servers from session update");
            }

            return null;
        }

        /// <summary>
        ///     Handles the avatar connecting event by processing the server SDP answer
        ///     and starting video streaming.
        /// </summary>
        private void HandleAvatarConnecting(SessionUpdateAvatarConnecting avatarConnecting)
        {
            if (avatarHandler == null)
            {
                logger.LogWarning("Avatar connecting event received but avatarHandler is null");
                return;
            }

            try
            {
                // The SDK's ServerSdp is Base64-encoded JSON (e.g., "eyJ..." = {"type":"answer","sdp":"..."})
                // AvatarClient.AvatarConnecting() expects decoded JSON string
                string serverSdp = avatarConnecting.ServerSdp;
                try
                {
                    string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(serverSdp));
                    logger.LogInformation("Server SDP Base64-decoded successfully");
                    serverSdp = decoded;
                }
                catch (FormatException)
                {
                    // Not Base64-encoded, use as-is
                    logger.LogInformation("Server SDP is not Base64-encoded, using as-is");
                }

                avatarHandler.ProcessServerSdpAnswer(serverSdp);
                logger.LogInformation("Server SDP answer processed, starting video streaming...");
                avatarHandler.StartVideoStreaming();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing avatar connecting event");
            }
        }

        private void HandleAudioDelta(SessionUpdateResponseAudioDelta audioDelta)
        {
            if (mode == ConnectionMode.Avatar)
            {
                // Avatar mode handles audio through WebRTC
                return;
            }

            if (audioDelta.Delta == null || audioDelta.Delta.ToMemory().Length == 0)
            {
                logger.LogWarning("Audio delta received but Delta is null or empty");
                return;
            }

            byte[] pcmData = audioDelta.Delta.ToArray();
            if (pcmData.Length > 0)
            {
                audioHandler.AddPlaybackData(pcmData);
            }
        }

        #endregion

        #region IAsyncDisposable Implementation

        /// <summary>
        ///     Asynchronously releases resources used by the assistant.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (disposed) return;

            // Stop event processing
            if (eventProcessingCts != null)
            {
                eventProcessingCts.Cancel();
                if (eventProcessingTask != null)
                {
                    try
                    {
                        await eventProcessingTask.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected
                    }
                }

                eventProcessingCts.Dispose();
            }

            // Disconnect audio handler
            audioHandler.OnAudioDataAvailable -= OnMicrophoneAudioAvailable;

            // Dispose session
            if (session != null)
            {
                session.Dispose();
                session = null;
            }

            disposed = true;
        }

        #endregion
    }
}
