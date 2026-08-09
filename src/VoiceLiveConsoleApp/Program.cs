// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text;
using Azure;
using System.Text.Json;
using System.Collections.Generic;
using Azure.Identity;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars.Streaming;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models;
using Com.Reseul.Azure.AI.VoiceLiveAPI.WebRtcAudio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using ServerMessageHandlerManager = Com.Reseul.Azure.AI.VoiceLiveAPI.Core.ServerMessageHandlerManager;
using AvatarMessageHandlerManager = Com.Reseul.Azure.AI.VoiceLiveAPI.Core.AvatarMessageHandlerManager;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{
    /// <summary>
    ///     Specifies the connection mode for the VoiceInfo Live API client.
    /// </summary>
    public enum ConnectionMode
    {
        /// <summary>
        ///     Direct connection to AI models (e.g., GPT-4o).
        /// </summary>
        AIModel,

        /// <summary>
        ///     Connection to custom AI agents.
        /// </summary>
        AIAgent,

        /// <summary>
        ///     Avatar mode with video streaming capabilities.
        /// </summary>
        Avatar,

        /// <summary>
        ///     2026-06-01-preview feature check: an AI Model session with a single preview feature enabled
        ///     so it can be exercised in isolation.
        /// </summary>
        FeatureCheck,

        /// <summary>
        ///     WebRTC voice connection: audio flows over WebRTC RTP media tracks via the
        ///     <c>/voice-live/realtime/calls</c> endpoint (2026-01-01-preview and later), instead of the
        ///     WebSocket <c>input_audio_buffer.append</c> / <c>response.audio.delta</c> path.
        /// </summary>
        WebRtcVoice
    }

    /// <summary>
    ///     Main console application class for the VoiceLive API sample application.
    ///     Provides interactive voice communication with Azure AI services.
    /// </summary>
    internal class Program
    {
        #region Public Methods

        /// <summary>
        ///     Main entry point of the console application.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [STAThread]
        private static async Task Main(string[] args)
        {
            // Set console encoding to UTF-8 to handle Japanese characters properly
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            ConsoleSettings.Initialize(config, args);

            if (ConsoleSettings.HelpRequested())
            {
                ConsoleSettings.PrintHelp();
                return;
            }

            // Logging is quiet by default (Error) so the feature output stands out. --wire-debug turns on the
            // full trace (every message plus the outgoing session.update JSON); --log-level picks anything in
            // between, with Information the useful middle ground when Debug buries what you want to watch.
            LogLevel minimumLevel = ConsoleSettings.GetFlag("WireDebug") ? LogLevel.Debug : LogLevel.Error;
            if (Enum.TryParse(ConsoleSettings.Get("LogLevel"), true, out LogLevel configured))
            {
                minimumLevel = configured;
            }

            ILoggerFactory loggerFactory = LoggerFactory.Create(configure =>
            {
                configure.SetMinimumLevel(minimumLevel);
                configure.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "[yyyy/MM/dd HH:mm:ss] ";
                });
            });

            LoggerFactoryManager.Set(loggerFactory);
            logger = LoggerFactoryManager.CreateLogger<Program>();

            // Both are created before logging is configured, so they get the logger here.
            audio.Logger = logger;
            avatar.Logger = logger;

            azureIdentityTokenRequestUrl = ConsoleSettings.GetOr("IdentityEndpoint", azureIdentityTokenRequestUrl);
            azureEndpoint = ConsoleSettings.GetOr("Endpoint", azureEndpoint);
            apiKey = ConsoleSettings.GetOr("ApiKey", apiKey);
            agentProjectName = ConsoleSettings.GetOr("AgentProjectName", agentProjectName);
            agentId = ConsoleSettings.GetOr("AgentId", agentId);
            agentName = ConsoleSettings.GetOr("AgentName", agentName);
            agentAccessToken = ConsoleSettings.GetOr("AgentAccessToken", agentAccessToken);
            modelName = ConsoleSettings.GetOr("Model", modelName);
            avatar.Backend = ConsoleSettings.GetOr("AvatarBackend", avatar.Backend);

            string apiVersion = ConsoleSettings.GetOr("ApiVersion", selectedApiVersion).Trim();
            if (Array.IndexOf(SupportedApiVersions, apiVersion) >= 0)
            {
                selectedApiVersion = apiVersion;
            }

            useStreamingTextInput = ConsoleSettings.GetFlag("StreamingText");
            AgentModelResolver.Enable(ConsoleSettings.GetFlag("ResolveAgentModel"));

            Console.WriteLine("Azure VoiceLive API Console Application");
            Console.WriteLine("Using VoiceLiveClient / VoiceLiveSession API");
            Console.WriteLine("============================================");

            try
            {
                // Choose what to run. The API version is only prompted inside the preview branch (that is
                // where it scopes the menu); standard modes use the default / the ApiVersion setting.
                ApplySelection(ConsoleMenu.Choose(SupportedApiVersions, selectedApiVersion, avatar.Backend,
                    ConsoleSettings.GetOr("WebRtcApiVersion", ApiVersion20260101)));

                // Initialize client based on mode
                await InitializeClientAsync();

                // WebRTC voice uses its own peer connection and audio endpoint, not the WebSocket/NAudio
                // path. Run it in isolation and exit when done.
                if (currentMode == ConnectionMode.WebRtcVoice)
                {
                    await RunWebRtcVoiceAsync();
                    return;
                }

                // Initialize audio components
                InitializeAudioPipeline();

                // Connect to VoiceLive API
                Console.WriteLine($"Connecting to Azure VoiceLive API in {currentMode} mode...");

                // Start session with VoiceLiveClient
                VoiceLiveSessionOptions sessionOptions = CreateSessionOptions(currentMode);

                if (!IsAgentSession(currentMode))
                {
                    voiceLiveSession = await voiceLiveClient!.StartSessionAsync(sessionOptions);
                }
                else
                {
                    // AI Agent mode or Avatar mode
                    voiceLiveSession = await voiceLiveClient!.StartAgentSessionByNameAsync(
                        agentProjectName, agentName, sessionOptions);
                }

                // Add message handlers to session
                SetupSessionEventHandlers();

                // Start audio input
                audio.StartRecording();

                Console.WriteLine("\nReady for conversation!");
                PrintCommands();

                if (currentMode == ConnectionMode.FeatureCheck)
                {
                    PrintFeatureCheckHint();
                }

                // Main loop
                bool running = true;
                while (running)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.R:
                            audio.ToggleRecording();
                            break;
                        case ConsoleKey.P:
                            audio.TogglePlayback();
                            break;
                        case ConsoleKey.M:
                            await SwitchMode();
                            break;
                        case ConsoleKey.C:
                            ClearAudioQueue();
                            break;
                        case ConsoleKey.S:
                            ShowStatus();
                            break;
                        case ConsoleKey.I:
                            await SendImageAsync();
                            break;
                        case ConsoleKey.X:
                            await SendTextInputAsync();
                            break;
                        case ConsoleKey.T:
                            await TestAndReconnect();
                            break;
                        case ConsoleKey.Q:
                            running = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error: {ex}", ex);
            }
            finally
            {
                await Cleanup();
            }
        }

        #endregion

        #region Static Fields and Constants

        /// <summary>
        ///     Audio sample rate in Hz for regular mode.
        /// </summary>
        private const int SampleRate = 24000;

        /// <summary>
        ///     How many seconds of assistant audio the playback buffer can hold. Response audio arrives faster
        ///     than real time, so this must exceed the longest single response; otherwise the overflow is
        ///     discarded and the answer is truncated mid-sentence.
        /// </summary>
        private const int PlaybackBufferSeconds = 120;

        /// <summary>
        ///     Number of audio channels for regular mode.
        /// </summary>
        private const int Channels = 1;

        /// <summary>
        ///     Bits per audio sample.
        /// </summary>
        private const int BitsPerSample = 16;

        /// <summary>
        ///     Audio sample rate in Hz for Avatar mode (Opus).
        /// </summary>
        private const int AvatarSampleRate = 48000;

        /// <summary>
        ///     Number of audio channels for Avatar mode (Opus).
        /// </summary>
        private const int AvatarChannels = 2;

        /// <summary>
        ///     The microphone and speakers, plus the barge-in gating that decides whose audio is still wanted.
        /// </summary>
        private static readonly AudioPipeline audio =
            new AudioPipeline(SampleRate, BitsPerSample, Channels, PlaybackBufferSeconds, null);

        /// <summary>Number of tool calls expected in the current tool-calling turn (parallel-safe).</summary>
        private static int expectedToolOutputs;

        /// <summary>Number of tool outputs submitted in the current tool-calling turn.</summary>
        private static int submittedToolOutputs;

        /// <summary>1 once the tool-calling response has completed (response.done) for the current turn.</summary>
        private static int toolResponseDoneFlag;

        /// <summary>Guard so the follow-up response is created exactly once per tool turn.</summary>
        private static int toolResponseCreated;

        /// <summary>
        ///     Logger instance for application logging.
        /// </summary>
        private static ILogger? logger;

        /// <summary>
        ///     Azure AI Services endpoint URL.
        /// </summary>
        private static string azureEndpoint = "<your Azure AI Services Endpoint>";

        /// <summary>
        ///     Azure AI Foundry project name for agent mode.
        /// </summary>
        private static string agentProjectName = "<your Azure AI Foundry Project Name>";

        /// <summary>
        ///     Azure AI agent identifier for agent mode (classic connection, deprecated 2026-08-31).
        /// </summary>
        private static string agentId = "<your Azure AI Agent Id>";

        /// <summary>
        ///     Azure AI agent name for the new (agent-name) connection method used by this sample.
        /// </summary>
        private static string agentName = "<your Azure AI Agent Name>";

        /// <summary>
        ///     The AI model to use in AI Model mode. Overridable with <c>VOICELIVE_MODEL</c>
        ///     (e.g. <c>gpt-4o</c>, <c>phi4-mm-realtime</c>, <c>gpt-realtime</c>).
        /// </summary>
        private static string modelName = VoiceLiveSessionOptions.DefaultModel;

        /// <summary>
        ///     Whether the 'X' text command delivers the typed text as a <b>pre-generated assistant message</b>
        ///     (the assistant speaks it verbatim; see <see cref="SendPreGeneratedAssistantMessageAsync" />)
        ///     instead of sending it as a user message. Enabled with <c>VOICELIVE_STREAMING_TEXT=1</c> or the
        ///     "streaming text input" feature check.
        /// </summary>
        private static bool useStreamingTextInput;

        /// <summary>
        ///     Whether microphone capture must be sent as interleaved stereo PCM16 (mic + speaker-playback echo
        ///     reference) for the client-side echo cancellation reference feature check.
        /// </summary>
        private static bool useStereoEcReference;

        /// <summary>
        ///     Builds the interleaved stereo frames when <see cref="useStereoEcReference" /> is active. Null
        ///     otherwise.
        /// </summary>
        private static EchoReferenceStereoCapture? echoReferenceCapture;

        /// <summary>
        ///     Token request URL for Azure Identity authentication.
        /// </summary>
        private static string azureIdentityTokenRequestUrl = "<Token request url(ex:https://ai.azure.com/.default)>";

        /// <summary>
        ///     Azure AI Foundry API key for authentication.
        /// </summary>
        private static string apiKey = "<Azure AI Foundry API Key>";

        /// <summary>
        ///     Access token for agent authentication.
        /// </summary>
        private static string agentAccessToken = "<Azure AI Foundry API Key>";

        /// <summary>
        ///     Server message handler manager for handling server responses.
        /// </summary>
        private static ServerMessageHandlerManager? serverManager;

        /// <summary>
        ///     The avatar half of the session: which avatar, which transport, and the pieces that carry its
        ///     video. Inert unless the chosen mode actually has an avatar.
        /// </summary>
        private static readonly AvatarSession avatar = new AvatarSession(null);

        /// <summary>
        ///     The preview feature to exercise in <see cref="ConnectionMode.FeatureCheck" /> mode, chosen from
        ///     <see cref="PreviewFeatureCatalog" />. Null until a feature check is selected.
        /// </summary>
        private static PreviewFeatureCheck? currentFeatureCheck;

        /// <summary>
        ///     Guards the one-time proactive greeting (for the proactive feature check) so it fires once per
        ///     session on the first <c>session.updated</c>.
        /// </summary>
        private static bool proactiveGreetingSent;

        /// <summary>
        ///     Whether the smart end-of-turn feature check is active. In that case the microphone is kept on
        ///     (the client does not auto-stop on the VAD's <c>speech_stopped</c>) so the smart end-of-turn
        ///     model can wait through natural pauses — otherwise the mic would cut before it can decide.
        /// </summary>
        private static bool SmartEouActive =>
            currentMode == ConnectionMode.FeatureCheck && currentFeatureCheck?.KeepMicOpen == true;

        /// <summary>
        ///     Current connection mode for reconnection purposes.
        /// </summary>
        private static ConnectionMode currentMode;

        /// <summary>
        ///     Indicates whether API Key authentication is used (false = EntraID/TokenCredential).
        /// </summary>
        private static bool useApiKeyAuth;


        /// <summary>
        ///     VoiceLiveClient instance for session management.
        /// </summary>
        private static VoiceLiveClient? voiceLiveClient;

        /// <summary>
        ///     VoiceLiveSession instance for real-time communication.
        /// </summary>
        private static VoiceLiveSession? voiceLiveSession;

        #endregion

        #region Private Methods

        /// <summary>
        ///     Creates VoiceLiveSessionOptions based on the connection mode.
        /// </summary>
        /// <param name="mode">The connection mode.</param>
        /// <returns>Configured session options.</returns>
        private static VoiceLiveSessionOptions CreateSessionOptions(ConnectionMode mode)
        {
            VoiceLiveSessionOptions? options = VoiceLiveSessionOptions.CreateDefault();

            switch (mode)
            {
                case ConnectionMode.AIModel:
                case ConnectionMode.AIAgent:
                case ConnectionMode.FeatureCheck:
                    // Standard audio settings - match ClientSessionUpdate.Default
                    options.Avatar = null;
                    // Model applies to AI Model / feature-check mode (agent mode uses agent-name).
                    options.Model = modelName;
                    // Ensure modalities include audio for audio output
                    options.Modalities = new[] { "text", "audio" };
                    options.InputAudioFormat = "pcm16";
                    options.OutputAudioFormat = "pcm16";
                    options.InputAudioSamplingRate = 24000;
                    // Voice configuration - required for audio output.
                    // Showcase: 2026-06-01-preview adds the azure-realtime-native voice type for
                    // azure-realtime models, e.g. new Voice { Name = "ava", Type = "azure-realtime-native" }.
                    // The default below uses an Azure standard HD voice (works with phi/gpt models here).
                    options.Voice = new Voice
                    {
                        Name = ConsoleSettings.GetOr("Voice", DefaultVoiceName),
                        Type = "azure-standard"
                    };
                    // Output audio timestamp types for word-level timing
                    options.OutputAudioTimestampTypes = new[] { "word" };
                    // Animation settings for viseme output
                    options.Animation = new Animation
                    {
                        Outputs = new[] { "viseme_id" }
                    };
                    // Turn detection (classic server VAD). Preview turn-detection variants (smart end-of-turn,
                    // auto-truncation) are demonstrated in FeatureCheck mode (see PreviewFeatureCatalog).
                    options.TurnDetection = new TurnDetection
                    {
                        Type = "server_vad",
                        Threshold = 0.5f, // Default threshold
                        SilenceDurationMs = 500, // Wait 500ms of silence before ending turn
                        CreateResponse = true // Explicitly enable automatic response generation
                    };
                    // Input audio noise reduction
                    options.InputAudioNoiseReduction = new AudioInputAudioNoiseReductionSettings
                    {
                        Type = "azure_deep_noise_suppression"
                    };

                    // Function Calling - サンプルツール定義
                    if (mode == ConnectionMode.AIModel || mode == ConnectionMode.FeatureCheck)
                    {
                        Function weatherTool = new Function
                        {
                            Name = "get_weather",
                            Description =
                                "Get the current weather for a given location. The user may ask in any language.",
                            Parameters = new Params
                            {
                                Properties = new Dictionary<string, Param>
                                {
                                    ["location"] = new Param
                                    {
                                        Type = "string",
                                        Description = "The city and country, e.g. 'Tokyo, Japan'"
                                    },
                                    ["unit"] = new Param
                                    {
                                        Type = "string",
                                        Enum = new[] { "celsius", "fahrenheit" },
                                        Description = "Temperature unit"
                                    }
                                },
                                Required = new[] { "location" }
                            }
                        };

                        // For the parallel-tool-calls check, add a second tool so both can be invoked in one turn.
                        options.Tools =
                            mode == ConnectionMode.FeatureCheck && currentFeatureCheck?.IncludeParallelToolSample == true
                                ? new[] { weatherTool, CreateGetTimeTool() }
                                : new[] { weatherTool };
                        options.ToolChoice = "auto";
                        // Showcase: allow the model to call multiple tools in parallel within a turn
                        // (2026-06-01-preview). Set to false for strictly sequential tool execution.
                        options.ParallelToolCalls = true;
                    }

                    // Feature-check mode: apply the selected preview feature's configuration.
                    if (mode == ConnectionMode.FeatureCheck)
                    {
                        currentFeatureCheck?.Apply(options);
                    }

                    break;

                case ConnectionMode.Avatar:
                    // Avatar mode - full settings like ClientSessionUpdate.Default.
                    // Model applies when Avatar runs on the Model backend (ignored on the Agent backend).
                    options.Model = modelName;
                    options.Modalities = new[] { "text", "audio" };
                    options.InputAudioFormat = "pcm16";
                    options.OutputAudioFormat = "pcm16";
                    options.InputAudioSamplingRate = 24000;
                    // Voice configuration. Avatar and voice are independent settings, so a personal voice
                    // can drive an avatar — pair a custom photo avatar with your own voice without the
                    // 10-minute studio recording a custom video avatar's voice-sync would need.
                    options.Voice = PreviewFeatureCatalog.TryBuildPersonalVoice()
                                    ?? new Voice
                                    {
                                        Name = ConsoleSettings.GetOr("Voice", DefaultVoiceName),
                                        Type = "azure-standard"
                                    };
                    if (options.Voice.Type == "azure-personal")
                    {
                        Console.WriteLine($"Avatar voice: personal voice ({options.Voice.Model})");
                    }
                    // Output audio timestamp types
                    options.OutputAudioTimestampTypes = new[] { "word" };
                    // Animation settings for viseme output
                    options.Animation = new Animation
                    {
                        Outputs = new[] { "viseme_id" }
                    };
                    // Turn detection
                    options.TurnDetection = new TurnDetection
                    {
                        Type = "server_vad",
                        Threshold = 0.5f,
                        SilenceDurationMs = 500,
                        CreateResponse = true
                    };
                    // Input audio noise reduction
                    options.InputAudioNoiseReduction = new AudioInputAudioNoiseReductionSettings
                    {
                        Type = "azure_deep_noise_suppression"
                    };
                    // Avatar video settings
                    options.Avatar = avatar.UsePhoto
                        ? BuildPhotoAvatarConfig()
                        : BuildVideoAvatarConfig();
                    break;
            }

            return options;
        }

        /// <summary>
        ///     Sets up event handlers for the VoiceLiveSession.
        /// </summary>
        private static void SetupSessionEventHandlers()
        {
            if (voiceLiveSession == null)
            {
                logger?.LogError("VoiceLiveSession is null, cannot set up event handlers");
                return;
            }

            // Initialize message handler managers
            serverManager = new ServerMessageHandlerManager();
            voiceLiveSession.AddMessageHandlerManager(serverManager);

            // The avatar brings its own transport and, for WebRTC, its own message handler.
            avatar.Attach(voiceLiveSession);

            // Set up event handlers
            SetupServerManagerEvents();

            logger?.LogInformation("Session event handlers configured");
        }

        /// <summary>
        ///     Subscribes to the server messages the console reacts to. Grouped by what each group is for:
        ///     one flat list of every event says nothing about which handler serves which concern.
        /// </summary>
        private static void SetupServerManagerEvents()
        {
            if (serverManager == null)
            {
                return;
            }

            WireMediaEvents(serverManager);
            WireSessionLifecycleEvents(serverManager);
            WireTurnControlEvents(serverManager);
            WireToolEvents(serverManager);
            WireDiagnosticEvents(serverManager);
        }

        /// <summary>
        ///     Playback and video: audio deltas reach the speakers (and the echo reference), and WebSocket avatar
        ///     frames reach FFplay.
        /// </summary>
        /// <param name="serverManager">The handler manager to subscribe on.</param>
        private static void WireMediaEvents(ServerMessageHandlerManager serverManager)
        {
            serverManager.OnAudioDeltaReceived += response =>
            {
                if (string.IsNullOrEmpty(response.Delta))
                {
                    logger?.LogWarning("Audio delta received but Delta is null or empty");
                    return;
                }

                // The pipeline takes it from here: it drops audio belonging to a response that was
                // interrupted, feeds the echo reference, and resumes playback when the speakers went idle.
                audio.EnqueueResponseAudio(response.ResponseId, Convert.FromBase64String(response.Delta));
            };

            // WebSocket avatar: decode each response.video.delta frame and render it via FFplay.
            serverManager.OnVideoDeltaReceived += response =>
            {
                if (string.IsNullOrEmpty(response.Delta))
                {
                    return;
                }

                try
                {
                    avatar.WriteVideoFrame(Convert.FromBase64String(response.Delta));
                }
                catch (FormatException ex)
                {
                    logger?.LogWarning(ex, "Video delta was not valid base64 (codec: {codec})", response.Codec);
                }
            };
        }

        /// <summary>
        ///     Session establishment: the proactive greeting, and the WebRTC avatar SDP/ICE negotiation that can only
        ///     happen once the service reports its ICE servers. Recording starts once this settles.
        /// </summary>
        /// <param name="serverManager">The handler manager to subscribe on.</param>
        private static void WireSessionLifecycleEvents(ServerMessageHandlerManager serverManager)
        {
serverManager.OnTranscriptionReceived += transcription =>
            {
                logger?.Log(LogLevel.Trace, "[message]: {Transcript}", transcription.Transcript);
            };

            serverManager.OnSessionUpdateReceived += async sessionUpdate =>
            {
                logger?.Log(LogLevel.Trace, "type : {Type}", sessionUpdate.Type);
                logger?.LogInformation("Session update received - Avatar: {hasAvatar}, IceServers: {hasIce}",
                    sessionUpdate.Avatar != null,
                    sessionUpdate.Avatar?.IceServers != null
                        ? sessionUpdate.Avatar.IceServers.Length.ToString()
                        : "null");

                // Proactive greeting feature check: on the first session.updated, have the assistant speak first
                // (a system 'greet the user' instruction + response.create).
                if (currentMode == ConnectionMode.FeatureCheck && currentFeatureCheck?.SendProactiveGreeting == true
                    && !proactiveGreetingSent && voiceLiveSession != null)
                {
                    proactiveGreetingSent = true;
                    try
                    {
                        // The default greeting is in Japanese to match the default ja-JP-Nanami voice; override
                        // the full instruction (any language) via VOICELIVE_GREETING.
                        string greetingText = ConsoleSettings.GetOr("Greeting",
                            "日本語で、ユーザーに手短に温かく挨拶し、続けて何かお手伝いできることを尋ねてください。");
                        await voiceLiveSession.SendMessageAsync(new
                        {
                            type = "conversation.item.create",
                            item = new
                            {
                                type = "message",
                                role = "system",
                                content = new[]
                                {
                                    new { type = "input_text", text = greetingText }
                                }
                            }
                        });
                        await voiceLiveSession.SendMessageAsync(new { type = "response.create" });
                        Console.WriteLine("[Feature Check] Proactive greeting requested (assistant speaks first).");
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Failed to send proactive greeting");
                    }
                }

                // The avatar negotiates its own transport: WebRTC needs the ICE servers reported here,
                // while the WebSocket transport needs nothing at all.
                if (voiceLiveSession != null)
                {
                    await avatar.ConnectWebRtcAsync(sessionUpdate.Avatar?.IceServers, voiceLiveSession);
                }

                audio.StartRecording();
            };
        }

        /// <summary>
        ///     Turn boundaries: barge-in and auto-truncation stop playback, and the VAD reporting stopped speech ends
        ///     the recording - except under smart end-of-turn, where stopping early would defeat the feature.
        /// </summary>
        /// <param name="serverManager">The handler manager to subscribe on.</param>
        private static void WireTurnControlEvents(ServerMessageHandlerManager serverManager)
        {
serverManager.OnConversationItemTruncatedReceived += DebugMessages;
            // Barge-in / auto-truncation: when the assistant's response is truncated, interrupt playback so the
            // cut-off audio stops and its late deltas are dropped instead of mixing with the new response.
            serverManager.OnConversationItemTruncatedReceived += _ => audio.Interrupt();
            serverManager.OnInputAudioBufferClearedReceived += DebugMessages;
            serverManager.OnInputAudioBufferCommittedReceived += DebugMessages;
            serverManager.OnInputAudioBufferSpeechStartedReceived += DebugMessages;
            // Barge-in: the moment the user starts speaking, interrupt playback (with interrupt_response the
            // server stops the in-progress response). The interrupted response's id is suppressed so its
            // in-flight audio deltas are dropped rather than re-buffered ahead of the new response.
            serverManager.OnInputAudioBufferSpeechStartedReceived += _ => audio.Interrupt();
            serverManager.OnInputAudioBufferSpeechStoppedReceived += DebugMessages;

            // Auto-stop recording when speech is detected as stopped.
            // Exception: for the smart end-of-turn check, keep the microphone on — auto-stopping on the VAD's
            // speech_stopped would cut the mic before the smart EOU model can wait through a natural pause,
            // defeating the very feature under test.
            serverManager.OnInputAudioBufferSpeechStoppedReceived += message =>
            {
                if (audio.IsRecording && !SmartEouActive)
                {
                    logger?.LogTrace("🔇 Speech stopped detected (audio_end: {ms}ms) - auto-stopping recording",
                        message.AudioEndMs);
                    audio.StopRecording();
                }
            };
        }

        /// <summary>
        ///     Tool execution: client-side function calls, and the hosted Foundry agent invocation events that appear
        ///     when an agent is wired in as a tool.
        /// </summary>
        /// <param name="serverManager">The handler manager to subscribe on.</param>
        private static void WireToolEvents(ServerMessageHandlerManager serverManager)
        {
serverManager.OnFoundryAgentCallInProgressReceived += e =>
            {
                Console.WriteLine($"[FoundryAgentCall] in_progress (item {e.ItemId}, output {e.OutputIndex})"
                                  + (string.IsNullOrEmpty(e.AgentResponseId)
                                      ? string.Empty
                                      : $", response {e.AgentResponseId}"));
                AgentModelResolver.Report(e.AgentResponseId);
            };
            serverManager.OnFoundryAgentCallArgumentsDeltaReceived += DebugMessages;
            serverManager.OnFoundryAgentCallArgumentsDoneReceived += DebugMessages;
            serverManager.OnFoundryAgentCallCompletedReceived += e =>
            {
                Console.WriteLine($"[FoundryAgentCall] completed (item {e.ItemId})");
                AgentModelResolver.Report(e.AgentResponseId);
            };
            serverManager.OnFoundryAgentCallFailedReceived += e =>
                Console.WriteLine($"[FoundryAgentCall] failed (item {e.ItemId})");

serverManager.OnFunctionCallDeltaReceived += delta =>
            {
                Console.WriteLine("[Function Call Delta] call_id={0}, delta={1}", delta.CallId, delta.Delta);
            };
            serverManager.OnFunctionCallDoneReceived += async done =>
            {
                Console.WriteLine("[Function Call Done] name={0}, call_id={1}, arguments={2}",
                    done.Name, done.CallId, done.Arguments);
                await HandleFunctionCallAsync(done);
            };
        }

        /// <summary>
        ///     Everything that only reports: transcripts, errors, and the trace of message types. A failed response is
        ///     surfaced here too - the reason arrives inside response.done rather than as an error event, so without
        ///     this the symptom would just be silence.
        /// </summary>
        /// <param name="serverManager">The handler manager to subscribe on.</param>
        private static void WireDiagnosticEvents(ServerMessageHandlerManager serverManager)
        {
serverManager.OnErrorReceived += response =>
            {
                logger?.LogError("Error received: {Type} - {Response}", response.Type,
                    JsonSerializer.Serialize(response));
            };

            serverManager.OnResponseTextDoneReceived += response =>
            {
                logger?.LogTrace("{Type} : {Text}", response.Type, response.Text);
            };

            serverManager.OnConversationCreatedReceived += DebugMessages;
            serverManager.OnConversationItemCreatedReceived += response =>
            {
                string transcripts = "";
                if (response.Item?.Content != null && response.Item.Content.Length > 0)
                {
                    transcripts = response.Item?.Content?.Select(x => x.Transcript).Aggregate((a, b) => a + "\n" + b) ??
                                  "";
                }

                logger?.LogTrace(" {role}: {transcripts}", response.Item?.Role, transcripts);
            };
            serverManager.OnConversationItemRetrievedReceived += DebugMessages;
            serverManager.OnConversationItemDeletedReceived += DebugMessages;
            serverManager.OnConversationItemInputAudioTranscriptionFailedReceived += DebugMessages;

serverManager.OnOutputAudioBufferClearedReceived += DebugMessages;
            serverManager.OnOutputAudioBufferStartedReceived += DebugMessages;
            serverManager.OnOutputAudioBufferStoppedReceived += DebugMessages;

            // Hosted agent invocation events (2026-06-01-preview): surfaced when a hosted Foundry agent is
            // invoked as a tool within the session. These fire only if the session is configured with a
            // Foundry-agent tool, so in the standard checks they simply won't appear.

serverManager.OnRateLimitsUpdatedReceived += DebugMessages;
            serverManager.OnResponseAnimationVisemeDoneReceived += DebugMessages;
            serverManager.OnResponseAnimationVisemeDeltaReceived += DebugMessages;
            serverManager.OnResponseAudioDoneReceived += DebugMessages;
            serverManager.OnResponseAudioTranscriptDeltaReceived += DebugMessages;
            serverManager.OnResponseAudioTranscriptDoneReceived += DebugMessages;
            serverManager.OnResponseContentPartAddedReceived += DebugMessages;
            serverManager.OnResponseContentPartDoneReceived += DebugMessages;
            serverManager.OnResponseCreatedReceived += DebugMessages;
            serverManager.OnResponseDoneReceived += DebugMessages;

            // A response can fail without any `error` event: the reason is carried inside response.done as
            // status="failed" + status_details.error (e.g. "Speech synthesis failed: you don't have access to
            // this personalVoiceName"). Surface it, otherwise the symptom is just silence.
            // In an agent session the model behind the agent is never named; the conversation id is the only
            // handle on the agent's own execution, so use it to look the answering model up.
            serverManager.OnResponseDoneReceived += response => AgentModelResolver.Report(response.ConversationId);

            serverManager.OnResponseDoneReceived += response =>
            {
                if (!string.Equals(response.Status, "failed", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                string details = response.StatusDetails == null
                    ? "(no details)"
                    : JsonSerializer.Serialize(response.StatusDetails);
                Console.WriteLine($"❌ Response failed: {details}");
                logger?.LogError("Response failed: {Details}", details);
            };
            serverManager.OnResponseDoneReceived += async _ =>
            {
                // If the just-completed response contained tool calls we're handling, mark it done and try
                // to create the single follow-up response (fires once all tool outputs of the turn are in).
                if (Volatile.Read(ref expectedToolOutputs) > 0)
                {
                    Interlocked.Exchange(ref toolResponseDoneFlag, 1);
                    await TryCreateToolTurnResponseAsync();
                }
            };

serverManager.OnResponseOutputItemAddedReceived += DebugMessages;
            serverManager.OnResponseOutputItemDoneReceived += response =>
            {
                Console.WriteLine(
                    " {0}: {1}", response.Item.Role,
                    response.Item.Content?.Select(x => x.Transcript).Aggregate((a, b) => a + "\n" + b));
            };
            serverManager.OnResponseTextDeltaReceived += DebugMessages;
            serverManager.OnSessionCreatedReceived += DebugMessages;
        }

        /// <summary>
        ///     Prompts for the avatar video transport and stores it in <see cref="avatar.UseWebSocketVideo" />.
        ///     WebRTC negotiates an SDP/ICE peer connection (media via SIPSorcery). WebSocket sets the avatar
        ///     config <c>output_protocol=websocket</c> so video frames arrive as <c>response.video.delta</c>
        ///     events on the session WebSocket (requires API version 2026-06-01-preview; no WebRTC).
        /// </summary>
        /// <summary>
        ///     Whether the given mode connects as a Foundry agent session (Entra ID required, conversation
        ///     managed server-side). Avatar defaults to an agent backend, but runs on a model session when
        ///     <c>VOICELIVE_AVATAR_BACKEND=model</c> — which enables model-only features such as image input.
        /// </summary>
        /// <param name="mode">The connection mode.</param>
        /// <returns><c>true</c> for an agent-backed session; otherwise <c>false</c>.</returns>
        private static bool IsAgentSession(ConnectionMode mode)
        {
            return mode == ConnectionMode.AIAgent
                   || (mode == ConnectionMode.Avatar && avatar.IsAgentBacked);
        }

        /// <summary>
        ///     Creates a second sample tool (<c>get_time</c>) used by the parallel-tool-calls feature check.
        /// </summary>
        /// <returns>A <c>get_time</c> function definition.</returns>
        private static Function CreateGetTimeTool()
        {
            return new Function
            {
                Name = "get_time",
                Description = "Get the current local time for a given location. The user may ask in any language.",
                Parameters = new Params
                {
                    Properties = new Dictionary<string, Param>
                    {
                        ["location"] = new Param
                        {
                            Type = "string",
                            Description = "The city and country, e.g. 'Tokyo, Japan'"
                        }
                    },
                    Required = new[] { "location" }
                }
            };
        }

        /// <summary>
        ///     Opens the microphone and speakers for the chosen mode, and points the capture at the session.
        /// </summary>
        /// <remarks>
        ///     The WebRTC avatar is the one mode that does not play the PCM response path locally: its audio
        ///     arrives as 48 kHz stereo Opus over the media stream instead. Every other mode, including the
        ///     WebSocket avatar, plays the standard 24 kHz path.
        /// </remarks>
        private static void InitializeAudioPipeline()
        {
            bool webRtcAvatar = avatar.IsEnabled && !avatar.UseWebSocketVideo;

            audio.Initialize(AvatarSampleRate, AvatarChannels, webRtcAvatar);
            audio.PlayResponseAudioLocally = !webRtcAvatar;
            audio.SendAudioAsync = data => voiceLiveSession?.SendInputAudioAsync(data) ?? Task.CompletedTask;
        }

        /// <summary>
        ///     Applies what the startup prompts chose. The one place the menu's answers become session state,
        ///     so the prompts themselves stay free of it.
        /// </summary>
        /// <param name="selection">What was chosen.</param>
        private static void ApplySelection(MenuSelection selection)
        {
            currentMode = selection.Mode;
            selectedApiVersion = selection.ApiVersion;
            avatar.IsEnabled = selection.Mode == ConnectionMode.Avatar;
            avatar.Backend = selection.AvatarBackend;
            avatar.UseWebSocketVideo = selection.AvatarUseWebSocketVideo;
            avatar.UsePhoto = selection.AvatarUsePhoto;
            currentFeatureCheck = selection.Feature;

            // The 'X' key and the microphone encoding follow the chosen feature.
            useStreamingTextInput = selection.Feature?.UseStreamingTextInput ?? useStreamingTextInput;
            useStereoEcReference = selection.Feature?.UseStereoEcReference ?? false;

            // The echo reference must retain at least the playback backlog, so size it like the playback buffer.
            echoReferenceCapture = useStereoEcReference
                ? new EchoReferenceStereoCapture(SampleRate, PlaybackBufferSeconds)
                : null;
        }

        /// <summary>
        ///     The Azure standard voice used when the <c>Voice</c> setting supplies nothing. Japanese, because
        ///     the sample's prompts and greeting are.
        /// </summary>
        private const string DefaultVoiceName = "ja-JP-Nanami:DragonHDLatestNeural";

        /// <summary>
        ///     The standard photo avatar characters ("Talking heads"). Used only to tell a standard character
        ///     from a custom one, so an unknown name is sent as <c>customized</c> instead of being rejected.
        /// </summary>
        private static readonly HashSet<string> StandardTalkingHeads = new HashSet<string>
        {
            "adrian", "amara", "amira", "anika", "bianca", "camila", "carlos", "clara", "darius", "diego",
            "elise", "farhan", "faris", "gabrielle", "hyejin", "imran", "isabella", "layla", "liwei", "ling",
            "marcus", "matteo", "rahul", "rana", "ren", "riya", "sakura", "simone", "zayd", "zoe"
        };

        /// <summary>
        ///     Builds the avatar config for the standard video avatar: a pre-rendered, full-body character,
        ///     cropped to the speaker so the 1920x1080 frame isn't mostly empty background.
        /// </summary>
        /// <returns>The <c>avatar</c> session object.</returns>
        private static Avatar BuildVideoAvatarConfig()
        {
            return new Avatar
            {
                Type = Avatar.Types.VideoAvatar,
                Character = "lisa",
                Style = "casual-sitting",
                Customized = false,
                OutputProtocol = avatar.OutputProtocol,
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
        ///     Builds the avatar config for a photo avatar: a single portrait animated by the vasa-1 base
        ///     model. The character comes from the "Talking heads" list rather than the full-body avatars,
        ///     those characters have no styles, and the frame is already a head shot — so no style and no
        ///     crop, unlike <see cref="BuildVideoAvatarConfig" />.
        /// </summary>
        /// <returns>The <c>avatar</c> session object.</returns>
        private static Avatar BuildPhotoAvatarConfig()
        {
            string character = ConsoleSettings.GetOr("PhotoAvatarCharacter", "sakura");

            // A name the service doesn't know is looked up against the standard characters and the session is
            // dropped with a bare error, so infer "custom" from the name rather than making the user set a
            // second setting in lockstep. PhotoAvatarCustomized still forces it on, which matters if a custom
            // avatar is ever named after a standard one.
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
                OutputProtocol = avatar.OutputProtocol,
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

        /// <summary>
        ///     Prints the keys worth knowing for the session that is actually running. Everything that only
        ///     applies to another mode is left out — the mode was already chosen in the menu — and the
        ///     diagnostic keys are collapsed onto one line so they don't crowd out the conversation.
        /// </summary>
        private static void PrintCommands()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("- 'R' record (auto-stops when you finish speaking)");
            Console.WriteLine("- 'X' send text"
                              + (useStreamingTextInput ? " (spoken verbatim as a pre-generated assistant message)" : string.Empty));

            // Images only reach a model session; agent sessions reject them.
            if (currentMode == ConnectionMode.AIModel || currentMode == ConnectionMode.FeatureCheck
                || (currentMode == ConnectionMode.Avatar && !IsAgentSession(currentMode)))
            {
                Console.WriteLine("- 'I' send an image");
            }

            Console.WriteLine("- 'Q' quit");
            Console.WriteLine("  (diagnostics: 'S' status, 'C' clear audio, 'P' playback, 'T' reconnect, 'M' switch mode)");
        }

        /// <summary>
        ///     Prints how to exercise the currently selected preview feature (feature-check mode).
        /// </summary>
        private static void PrintFeatureCheckHint()
        {
            if (currentFeatureCheck == null)
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"[Feature Check] {currentFeatureCheck.Id}");
            foreach (var line in currentFeatureCheck.HintLines)
            {
                Console.WriteLine(line);
            }

            Console.WriteLine();
        }

        /// <summary>
        ///     The wire API version this sample targets. 2026-06-01-preview adds the newer Voice Live
        ///     features showcased here (WebSocket avatar via <c>response.video.delta</c>, agent-name
        ///     connection, smart end-of-turn, streaming text input, azure-realtime-native voice,
        ///     parallel tool calls). The Core default is left unchanged; this console opts in explicitly.
        /// </summary>
        private const string PreviewApiVersion = "2026-06-01-preview";

        /// <summary>The earlier preview wire version selectable at startup.</summary>
        private const string ApiVersion20260101 = "2026-01-01-preview";

        /// <summary>The preview wire versions this console can target, oldest first.</summary>
        private static readonly string[] SupportedApiVersions = { ApiVersion20260101, PreviewApiVersion };

        /// <summary>
        ///     The preview wire version selected at startup. Both the client options and the feature-check
        ///     menu are scoped to this version so only version-appropriate features are shown.
        /// </summary>
        private static string selectedApiVersion = PreviewApiVersion;

        /// <summary>
        ///     Creates client options pinned to <see cref="selectedApiVersion" />.
        /// </summary>
        /// <returns>Client options with the selected preview API version.</returns>
        private static VoiceLiveClientOptions CreateClientOptions()
        {
            var options = new VoiceLiveClientOptions { ApiVersion = selectedApiVersion };

            // Opt into preview URL feature flags declared by the selected feature check (e.g.
            // client_ec_reference:true for client-side echo cancellation reference).
            if (currentMode == ConnectionMode.FeatureCheck
                && currentFeatureCheck?.WireFeatures is { Length: > 0 } features)
            {
                options.Features = features;
            }

            return options;
        }

        /// <summary>
        ///     Initializes the VoiceLive API client based on the specified mode.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static Task InitializeClientAsync()
        {
            useApiKeyAuth = ConsoleMenu.ChooseUseApiKey();

            try
            {
                if (useApiKeyAuth)
                {
                    // API Key authentication using AzureKeyCredential
                    logger?.LogInformation("Initializing VoiceLiveClient with API Key authentication...");
                    voiceLiveClient = new VoiceLiveClient(azureEndpoint, new AzureKeyCredential(apiKey), CreateClientOptions());
                }
                else
                {
                    // Entra ID authentication using DefaultAzureCredential
                    logger?.LogInformation("Initializing VoiceLiveClient with Entra ID authentication...");
                    voiceLiveClient = new VoiceLiveClient(
                        azureEndpoint,
                        new DefaultAzureCredential(),
                        new[] { azureIdentityTokenRequestUrl },
                        CreateClientOptions());
                }

                // Set agent configuration
                voiceLiveClient.AgentProjectName = agentProjectName;
                voiceLiveClient.AgentId = agentId;
                voiceLiveClient.AgentAccessToken = agentAccessToken;

                logger?.LogInformation("VoiceLiveClient initialized successfully");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to initialize VoiceLiveClient");
                throw;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Runs the WebRTC voice connection: negotiates a peer connection over the <c>/calls</c> endpoint,
        ///     streams conversation audio over RTP (mic up / speaker down), prints connection-state and
        ///     data-channel events, and waits until the user quits. Reuses the initialized
        ///     <see cref="voiceLiveClient" /> (endpoint + credential + selected API version).
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task RunWebRtcVoiceAsync()
        {
            // WebRTC voice streams native audio over RTP. The official WebRTC sample uses the azure-realtime
            // model with the azure-realtime-native voice (a cascaded Azure-TTS voice makes the service fail to
            // allocate the media client). Default to that proven combo; honor an explicit VOICELIVE_MODEL.
            string webrtcModel = ConsoleSettings.GetOr("Model", "azure-realtime");
            bool isAzureRealtime = string.Equals(webrtcModel, "azure-realtime", StringComparison.OrdinalIgnoreCase);

            Console.WriteLine(
                $"Connecting WebRTC voice ({selectedApiVersion}, model {webrtcModel}) via /voice-live/realtime/calls...");

            // Minimal session config for the rtc.call.sdp.create `session` field, matching the official WebRTC
            // sample: modalities + instructions + voice + turn_detection ONLY. The model is passed via the URL
            // (not the session), and WebSocket-audio fields (input_audio_format / sampling_rate) are omitted so
            // the service allocates an RTP media client instead of a pcm16 one.
            object voice = isAzureRealtime
                ? (object)new { type = "azure-realtime-native", name = "ava" }
                : new { type = "azure-standard", name = "en-US-AvaNeural" };
            object sessionConfig = new
            {
                modalities = new[] { "text", "audio" },
                instructions = "You are a helpful assistant. Respond concisely.",
                voice,
                turn_detection = new
                {
                    type = "server_vad",
                    threshold = 0.5,
                    prefix_padding_ms = 300,
                    silence_duration_ms = 500
                }
            };

            using var call = new VoiceLiveWebRtcCall(LoggerFactoryManager.CreateLogger<VoiceLiveWebRtcCall>());
            call.OnConnectionStateChanged += state => Console.WriteLine($"[WebRTC] connection state: {state}");
            call.OnDataChannelMessage += text => Console.WriteLine($"[WebRTC][event] {text}");
            call.OnCallError += err => Console.WriteLine(
                $"[WebRTC][error] {err.Operation}: {err.Error?.Code} {err.Error?.Message}");

            try
            {
                // The WebRTC /calls media allocation authenticates via the api-key URL query parameter, so
                // pass the key through when API Key auth is selected (Entra bearer authenticates only the
                // handshake and fails allocation).
                string? apiKeyForQuery = useApiKeyAuth ? apiKey : null;
                await call.ConnectAsync(voiceLiveClient!, webrtcModel, sessionConfig,
                    cancellationToken: default, apiKeyForQuery: apiKeyForQuery);
                Console.WriteLine("WebRTC voice: SDP answer applied. Waiting for ICE to reach 'connected'...");
                Console.WriteLine("Speak into your microphone once connected. Press 'S' for state, 'Q' to quit.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "WebRTC voice connection failed");
                Console.WriteLine($"WebRTC voice connection failed: {ex.Message}");
                return;
            }

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Q)
                {
                    break;
                }

                if (key.Key == ConsoleKey.S)
                {
                    Console.WriteLine($"[WebRTC] current state: {call.ConnectionState}");
                }
            }

            Console.WriteLine("Closing WebRTC voice connection...");
        }

        /// <summary>
        ///     Switches the connection mode and reinitializes the client.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task SwitchMode()
        {
            try
            {
                Console.WriteLine("\nSwitching mode...");

                // Disconnect current session
                audio.StopRecording();
                audio.StopPlayback();

                // Cleanup Avatar audio resources before switching
                audio.ReleaseAvatarBuffer();

                avatar.Dispose();

                // Dispose current session
                if (voiceLiveSession != null)
                {
                    await voiceLiveSession.DisposeAsync();
                    voiceLiveSession = null;
                }

                // Choose new mode
                ApplySelection(ConsoleMenu.Choose(SupportedApiVersions, selectedApiVersion, avatar.Backend,
                    ConsoleSettings.GetOr("WebRtcApiVersion", ApiVersion20260101)));
                ConnectionMode newMode = currentMode;

                // Initialize audio for new mode
                InitializeAudioPipeline();

                // Initialize new client
                await InitializeClientAsync();

                // Start new session
                Console.WriteLine($"Reconnecting in {newMode} mode...");
                VoiceLiveSessionOptions sessionOptions = CreateSessionOptions(newMode);

                if (!IsAgentSession(newMode))
                {
                    voiceLiveSession = await voiceLiveClient!.StartSessionAsync(sessionOptions);
                }
                else
                {
                    voiceLiveSession = await voiceLiveClient!.StartAgentSessionByNameAsync(
                        agentProjectName, agentName, sessionOptions);
                }

                SetupSessionEventHandlers();
                audio.StartRecording();

                Console.WriteLine("Mode switched successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error switching mode: {ex.Message}");
            }
        }

        /// <summary>
        ///     Handles a function call from the AI model and sends back the result.
        /// </summary>
        private static async Task HandleFunctionCallAsync(FunctionCallDone functionCallDone)
        {
            if (voiceLiveSession == null)
            {
                logger?.LogWarning("Cannot handle function call: session is null");
                return;
            }

            // Count this call as expected for the current tool turn (before any await) so parallel calls are
            // all accounted for before we decide whether to create the follow-up response.
            Interlocked.Increment(ref expectedToolOutputs);

            string output;

            switch (functionCallDone.Name)
            {
                case "get_weather":
                    output = HandleGetWeather(functionCallDone.Arguments);
                    break;
                case "get_time":
                    output = HandleGetTime(functionCallDone.Arguments);
                    break;
                default:
                    logger?.LogWarning("Unknown function: {Name}", functionCallDone.Name);
                    output = JsonSerializer.Serialize(new { error = $"Unknown function: {functionCallDone.Name}" });
                    break;
            }

            Console.WriteLine("[Function Call] {0}({1}) => {2}", functionCallDone.Name, functionCallDone.Arguments,
                output);

            // Send the function call output back to the server. Do NOT create a response here — for parallel
            // tool calls that would fire multiple times. The single follow-up response is created once, after
            // the tool-calling response is done AND every tool output of the turn is submitted.
            await voiceLiveSession.SendFunctionCallOutputAsync(functionCallDone.CallId, output);
            Interlocked.Increment(ref submittedToolOutputs);

            logger?.LogInformation("Function call output sent for call_id={CallId}", functionCallDone.CallId);

            await TryCreateToolTurnResponseAsync();
        }

        /// <summary>
        ///     Creates the follow-up response for a tool-calling turn exactly once — only after the
        ///     tool-calling response has completed (<c>response.done</c>) and every expected tool output for
        ///     the turn has been submitted. Robust to parallel calls and to slow/asynchronous tools whose
        ///     output is submitted after <c>response.done</c> (whichever completes last triggers it).
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task TryCreateToolTurnResponseAsync()
        {
            if (voiceLiveSession == null)
            {
                return;
            }

            // Require: the tool-calling response is done, at least one tool call was expected, and all
            // expected outputs have been submitted.
            if (Volatile.Read(ref toolResponseDoneFlag) == 0)
            {
                return;
            }

            int expected = Volatile.Read(ref expectedToolOutputs);
            if (expected <= 0 || Volatile.Read(ref submittedToolOutputs) < expected)
            {
                return;
            }

            // Fire exactly once for this turn (multiple callers may satisfy the condition concurrently).
            if (Interlocked.CompareExchange(ref toolResponseCreated, 1, 0) != 0)
            {
                return;
            }

            try
            {
                await voiceLiveSession.CreateResponseAsync();
                logger?.LogInformation("Tool turn complete ({n} outputs); response requested.", expected);
            }
            finally
            {
                // Reset for the next tool turn.
                Interlocked.Exchange(ref expectedToolOutputs, 0);
                Interlocked.Exchange(ref submittedToolOutputs, 0);
                Interlocked.Exchange(ref toolResponseDoneFlag, 0);
                Interlocked.Exchange(ref toolResponseCreated, 0);
            }
        }

        /// <summary>
        ///     Sample implementation of the get_weather function.
        /// </summary>
        private static string HandleGetTime(string argumentsJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(argumentsJson);
                var root = doc.RootElement;
                var location = root.TryGetProperty("location", out var loc) ? loc.GetString() : "Unknown";

                // Return mock time data for parallel-tool-call verification.
                var timeData = new
                {
                    location,
                    time = "14:30",
                    timezone = "local",
                    description = $"Local time for {location} (mock data for parallel tool call verification)"
                };

                return JsonSerializer.Serialize(timeData);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error parsing time arguments");
                return JsonSerializer.Serialize(new { error = "Failed to parse arguments", message = ex.Message });
            }
        }

        private static string HandleGetWeather(string argumentsJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(argumentsJson);
                var root = doc.RootElement;
                var location = root.TryGetProperty("location", out var loc) ? loc.GetString() : "Unknown";
                var unit = root.TryGetProperty("unit", out var u) ? u.GetString() : "celsius";

                // Return mock weather data for verification
                var weatherData = new
                {
                    location,
                    temperature = unit == "fahrenheit" ? 72 : 22,
                    unit,
                    condition = "sunny",
                    humidity = 45,
                    description = $"Weather data for {location} (mock data for function calling verification)"
                };

                return JsonSerializer.Serialize(weatherData);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error parsing weather arguments");
                return JsonSerializer.Serialize(new { error = "Failed to parse arguments", message = ex.Message });
            }
        }

        /// <summary>
        ///     Sends an image to the AI model for analysis.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task SendImageAsync()
        {
            // Image input works on a Model session (AI Model mode, or Avatar with the Model backend). Agent
            // sessions reject it with a server error (and drop the session) — confirmed on both
            // 2026-01-01-preview and 2026-06-01-preview — so block agent-backed sessions.
            if (IsAgentSession(currentMode))
            {
                Console.WriteLine("Image input is not supported on Agent sessions (they reject images). Use AI Model mode, or Avatar with the Model backend.");
                return;
            }

            if (voiceLiveSession == null)
            {
                Console.WriteLine("Session is not initialized.");
                return;
            }

            // Stop recording temporarily to allow console input
            bool wasRecording = audio.IsRecording;
            if (wasRecording)
            {
                audio.StopRecording();
            }

            Console.WriteLine("\n=== Image Input ===");
            Console.WriteLine("Enter image file path or URL (or 'cancel' to abort):");
            Console.Write("> ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input) || input.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Image input cancelled.");
                if (wasRecording)
                {
                    audio.StartRecording();
                }
                return;
            }

            Console.WriteLine("Enter a text prompt to accompany the image (optional, press Enter to skip):");
            Console.Write("> ");
            string? prompt = Console.ReadLine()?.Trim();

            try
            {
                string imageUrl;

                // Determine if input is a URL or file path
                if (input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    input.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                    input.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    imageUrl = input;
                    Console.WriteLine("Using URL: {0}", imageUrl.Length > 80 ? imageUrl.Substring(0, 80) + "..." : imageUrl);
                }
                else
                {
                    // Treat as file path
                    Console.WriteLine("Loading image from file: {0}", input);
                    imageUrl = ImageInputExtensions.CreateImageDataUri(input);
                    Console.WriteLine("Image converted to base64 data URI ({0} chars)", imageUrl.Length);
                }

                // Send image with or without text prompt
                if (!string.IsNullOrEmpty(prompt))
                {
                    await voiceLiveSession.SendImageWithTextAsync(imageUrl, prompt);
                    Console.WriteLine("[Image Sent] with text: \"{0}\"", prompt);
                }
                else
                {
                    await voiceLiveSession.SendImageAsync(imageUrl);
                    Console.WriteLine("[Image Sent] (no text prompt)");
                }

                // Trigger a response from the model
                await voiceLiveSession.CreateResponseAsync();
                Console.WriteLine("Response requested.");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Error: File not found - {0}", ex.FileName);
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending image: {0}", ex.Message);
                logger?.LogError(ex, "Error sending image");
            }

            // Resume recording if it was active
            if (wasRecording)
            {
                audio.StartRecording();
            }
        }

        private static void DebugMessages(ServerEvent response)
        {
            logger?.LogTrace("received: {Type}", response.Type);
        }

        private static void DebugMessages(MessageBase response)
        {
            logger?.LogTrace("received: {Type}", response.Type);
        }

        /// <summary>
        ///     Clears the audio queue in the VoiceLive session.
        /// </summary>
        private static void ClearAudioQueue()
        {
            if (voiceLiveSession != null)
            {
                Console.WriteLine("Clearing audio queue...");
                voiceLiveSession.ClearAudioQueue();
                Console.WriteLine("Audio queue cleared");
            }
            else
            {
                Console.WriteLine("Session not initialized");
            }
        }

        /// <summary>
        ///     Shows the current status of the application and audio components.
        /// </summary>
        private static void ShowStatus()
        {
            Console.WriteLine("\n=== Current Status ===");
            Console.WriteLine($"Recording: {(audio.IsRecording ? "ON" : "OFF")}");
            Console.WriteLine($"Playback: {(audio.IsPlaying ? "ON" : "OFF")}");
            Console.WriteLine($"Connection Mode: {currentMode}");
            Console.WriteLine($"Auth Method: {(useApiKeyAuth ? "API Key" : "Entra ID")}");

            if (voiceLiveSession != null)
            {
                Console.WriteLine($"Connected: {voiceLiveSession.IsConnected}");
                Console.WriteLine($"Endpoint: {voiceLiveClient?.Endpoint ?? "N/A"}");

                int queueCount = voiceLiveSession.AudioQueueCount;
                Console.WriteLine($"Audio Queue: {queueCount} chunks");
            }
            else
            {
                Console.WriteLine("Session: Not initialized");
            }

            if (echoReferenceCapture != null)
            {
                Console.WriteLine($"EC reference: {echoReferenceCapture.DescribeStats(SampleRate)}");
            }

            Console.WriteLine("=====================\n");
        }

        /// <summary>
        ///     Performs cleanup operations before application exit.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task Cleanup()
        {
            Console.WriteLine("Cleaning up...");

            audio.StopRecording();
            audio.StopPlayback();

            audio.ReleaseAvatarBuffer();
            audio.Dispose();

            avatar.Dispose();

            // Dispose VoiceLiveSession
            if (voiceLiveSession != null)
            {
                await voiceLiveSession.DisposeAsync();
                voiceLiveSession = null;
            }

            voiceLiveClient = null;

            Console.WriteLine("Goodbye!");
        }


        /// <summary>
        ///     Sends a typed text message to the session and requests a response.
        ///     Default path: send the text as a user <c>conversation.item.create</c> (<c>input_text</c>) and
        ///     request a model response via <see cref="VoiceLiveSession.SendUserMessageAsync" />.
        ///     When <see cref="useStreamingTextInput" /> is set (the "streaming text input" feature check), the
        ///     typed text is instead delivered as a <b>pre-generated assistant message</b>
        ///     (<see cref="SendPreGeneratedAssistantMessageAsync" />): the service speaks the predefined text
        ///     verbatim as the assistant's own line (bring-your-own assistant text → TTS) rather than answering.
        ///     See that method for why the raw <c>input_text.delta</c>/<c>.done</c> streaming variant is not
        ///     used here.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task SendTextInputAsync()
        {
            if (voiceLiveSession == null)
            {
                Console.WriteLine("⚠️ Session is not started.");
                return;
            }

            // Pause the microphone while reading console input so the server VAD does not fire on the
            // prompt; resume afterwards if it was active.
            bool wasRecording = audio.IsRecording;
            if (wasRecording)
            {
                audio.StopRecording();
            }

            try
            {
                Console.Write("Enter text to send (blank = sample): ");
                string? text = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = "こんにちは。今日の東京の天気を教えてください。";
                }

                if (useStreamingTextInput)
                {
                    // Pre-generated assistant message: the response.create itself carries the text, so we do
                    // NOT issue a separate response.create afterwards.
                    await SendPreGeneratedAssistantMessageAsync(text);
                }
                else
                {
                    // Reliable path: send the text as a user conversation item, then request a response.
                    await voiceLiveSession.SendUserMessageAsync(text);
                    await voiceLiveSession.CreateResponseAsync();
                }

                Console.WriteLine($"📝 Text input sent ({text.Length} chars, " +
                                  $"{(useStreamingTextInput ? "pre-generated assistant message" : "conversation.item.create")}).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending text: {ex.Message}");
                logger?.LogError(ex, "Error sending text input");
            }
            finally
            {
                if (wasRecording)
                {
                    audio.StartRecording();
                }
            }
        }

        /// <summary>
        ///     Delivers <paramref name="text" /> as a <b>pre-generated assistant message</b>: the service
        ///     speaks the predefined text verbatim (TTS) as the assistant's own line and adds it to the
        ///     conversation history, instead of the model generating a reply. This is the documented, working
        ///     form of the 2026-06-01-preview text-input feature — a single <c>response.create</c> carrying a
        ///     <c>pre_generated_assistant_message</c> (one text content entry only), so the caller must NOT
        ///     issue a separate <c>response.create</c>.
        ///     WHY NOT <c>input_text.delta</c>/<c>.done</c>: those are presumably the streaming version of this
        ///     (incremental append to an <i>incomplete</i> pre_generated_assistant_message), but on
        ///     2026-06-01-preview the way to open that incomplete item from a client is not yet documented — a
        ///     preview gap, not a confirmed impossibility (see the numbered error progression in the body).
        /// </summary>
        /// <param name="text">The predefined assistant text to speak.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task SendPreGeneratedAssistantMessageAsync(string text)
        {
            if (voiceLiveSession == null)
            {
                return;
            }

            // Only the one-shot form is implemented. input_text.delta/.done is presumably the STREAMING version
            // of this pre-generated assistant message (incremental append to an *incomplete*
            // pre_generated_assistant_message), but on 2026-06-01-preview its usage is not yet documented — the
            // way to open that incomplete item from a client is unknown, so it is left unimplemented (a preview
            // gap, not confirmed impossible). Empirical error progression when trying it (kept as evidence):
            //   1. input_text.delta {type, delta}                       -> "Missing required parameter: 'id'".
            //   2. + client-assigned id (no item.create)                -> "No incomplete pre_generated_assistant_message found".
            //   3. open via response.create, client id + content=[]      -> same "No incomplete..." (client id ignored).
            //   4. open via response.create with NO content              -> "Missing required parameter: 'content'".
            // (For reference, hosted agents stream spoken text via a different event, output_audio_transcription.delta;
            // input_text.delta has zero usages in the official samples.) The reference's "streamed user-text
            // input" wording and its id-less input_text.delta example look like preview doc gaps.
            await voiceLiveSession.SendMessageAsync(new
            {
                type = "response.create",
                response = new
                {
                    pre_generated_assistant_message = new
                    {
                        type = "message",
                        role = "assistant",
                        content = new[] { new { type = "text", text } }
                    }
                }
            });
        }

        /// <summary>
        ///     Tests connection and reconnects if needed.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task TestAndReconnect()
        {
            try
            {
                Console.WriteLine("\n🔄 Testing connection...");

                // Check connection state
                bool isConnected = TestConnection();

                if (isConnected)
                {
                    Console.WriteLine("✅ Connection is healthy");
                    return;
                }

                Console.WriteLine("🔧 Connection issues detected, attempting reconnection...");

                // Stop current activities
                audio.StopRecording();
                audio.StopPlayback();

                // Cleanup Avatar audio resources before reconnection
                audio.ReleaseAvatarBuffer();

                avatar.Dispose();

                // Dispose current session
                if (voiceLiveSession != null)
                {
                    try
                    {
                        await voiceLiveSession.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Error disposing session during reconnection");
                    }

                    voiceLiveSession = null;
                }

                // Wait a moment before reconnecting
                await Task.Delay(1000);

                // Recreate client with stored credentials
                RecreateClient();

                // Start new session
                Console.WriteLine($"🔄 Reconnecting in {currentMode} mode...");
                VoiceLiveSessionOptions sessionOptions = CreateSessionOptions(currentMode);

                if (!IsAgentSession(currentMode))
                {
                    voiceLiveSession = await voiceLiveClient!.StartSessionAsync(sessionOptions);
                }
                else
                {
                    voiceLiveSession = await voiceLiveClient!.StartAgentSessionByNameAsync(
                        agentProjectName, agentName, sessionOptions);
                }

                SetupSessionEventHandlers();

                // Restart recording
                audio.StartRecording();

                Console.WriteLine("✅ Reconnection successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Reconnection failed: {ex.Message}");
                logger?.LogError(ex, "Error during reconnection");
            }
        }

        /// <summary>
        ///     Tests if the current connection is healthy.
        /// </summary>
        /// <returns>True if connection is healthy, false otherwise.</returns>
        private static bool TestConnection()
        {
            try
            {
                if (voiceLiveSession == null)
                    return false;

                // Check if session is connected
                if (!voiceLiveSession.IsConnected)
                    return false;

                // Try to clear audio queue as a simple connection test
                voiceLiveSession.ClearAudioQueue();

                return true;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Connection test failed");
                return false;
            }
        }

        /// <summary>
        ///     Recreates the VoiceLiveClient with stored credentials.
        /// </summary>
        private static void RecreateClient()
        {
            try
            {
                logger?.LogInformation("Recreating VoiceLiveClient...");

                if (useApiKeyAuth)
                {
                    voiceLiveClient = new VoiceLiveClient(azureEndpoint, new AzureKeyCredential(apiKey), CreateClientOptions());
                }
                else
                {
                    voiceLiveClient = new VoiceLiveClient(
                        azureEndpoint,
                        new DefaultAzureCredential(),
                        new[] { azureIdentityTokenRequestUrl },
                        CreateClientOptions());
                }

                voiceLiveClient.AgentProjectName = agentProjectName;
                voiceLiveClient.AgentId = agentId;
                voiceLiveClient.AgentAccessToken = agentAccessToken;

                logger?.LogInformation("VoiceLiveClient recreated successfully");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error recreating client");
                throw;
            }
        }

        #endregion
    }
}