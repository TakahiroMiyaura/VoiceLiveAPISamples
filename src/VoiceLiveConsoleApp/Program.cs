// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using Azure;
using Azure.Identity;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models;
using Concentus;
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
        ///     AI Model with Foundry Agent Tool (Preview API 2026-01-01-preview).
        ///     Chat supervisor pattern: AI Model calls Foundry Agent as a tool.
        /// </summary>
        AIModelWithFoundryAgent,

        /// <summary>
        ///     Connection to custom AI agents.
        /// </summary>
        AIAgent,

        /// <summary>
        ///     Avatar mode with video streaming capabilities.
        /// </summary>
        Avatar
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
        private static async Task Main()
        {
            // Set console encoding to UTF-8 to handle Japanese characters properly
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            ILoggerFactory loggerFactory = LoggerFactory.Create(configure =>
            {
                configure.SetMinimumLevel(LogLevel.Error);
                configure.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "[yyyy/MM/dd HH:mm:ss] ";
                });
            });

            LoggerFactoryManager.Set(loggerFactory);
            logger = LoggerFactoryManager.CreateLogger<Program>();


            IConfigurationRoot config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            azureIdentityTokenRequestUrl = config["Identity:AzureEndpoint"] ?? azureIdentityTokenRequestUrl;
            azureEndpoint = config["VoiceLiveAPI:AzureEndpoint"] ?? azureEndpoint;
            apiKey = config["AzureAIFoundry:ApiKey"] ?? apiKey;
            agentProjectName = config["AzureAIFoundry:AgentProjectName"] ?? agentProjectName;
            agentId = config["AzureAIFoundry:AgentId"] ?? agentId;
            agentAccessToken = config["AzureAIFoundry:AgentAccessToken"] ?? agentAccessToken;

            Console.WriteLine("Azure VoiceLive API Console Application");
            Console.WriteLine("Using VoiceLiveClient / VoiceLiveSession API");
            Console.WriteLine("============================================");

            try
            {
                // Choose connection mode
                currentMode = ChooseConnectionMode();

                // Prompt for Foundry Agent tool configuration (AI Model + Foundry Agent mode)
                if (currentMode == ConnectionMode.AIModelWithFoundryAgent)
                {
                    PromptFoundryAgentToolConfig();
                }

                // Initialize client based on mode
                await InitializeClientAsync();

                // Initialize audio components
                InitializeAudio();

                // Connect to VoiceLive API
                Console.WriteLine($"Connecting to Azure VoiceLive API in {currentMode} mode...");

                // Start session with VoiceLiveClient
                VoiceLiveSessionOptions sessionOptions = CreateSessionOptions(currentMode);

                // Diagnostic: Print the session.update JSON that will be sent
                PrintSessionDiagnostics(sessionOptions);

                if (currentMode == ConnectionMode.AIModel || currentMode == ConnectionMode.AIModelWithFoundryAgent)
                {
                    voiceLiveSession = await voiceLiveClient!.StartSessionAsync(sessionOptions);
                }
                else
                {
                    // AI Agent mode or Avatar mode
                    voiceLiveSession = await voiceLiveClient!.StartAgentSessionAsync(
                        agentProjectName, agentId, sessionOptions);
                }

                // Add message handlers to session
                SetupSessionEventHandlers();

                // Note: Recording is started by OnSessionUpdateReceived handler
                // after the server confirms the session is ready (session.updated event).
                // Starting here would cause a race condition with NAudio's internal state.

                Console.WriteLine("\nReady for conversation!");
                Console.WriteLine("Commands:");
                Console.WriteLine("- Press 'R' to start/stop recording");
                Console.WriteLine("- Press 'P' to start/stop playback");
                Console.WriteLine("- Press 'M' to switch mode and authentication (requires reconnection)");
                Console.WriteLine("- Press 'C' to clear audio queue");
                Console.WriteLine("- Press 'S' to show detailed status");
                Console.WriteLine("- Press 'I' to send an image (AI Model mode only)");
                Console.WriteLine("- Press 'V' to toggle avatar video streaming (Avatar mode only)");
                Console.WriteLine("- Press 'F' to show avatar streaming information (Avatar mode only)");
                Console.WriteLine("- Press 'T' to test connection and reconnect if needed");
                Console.WriteLine("- Press 'Q' to quit");

                // Main loop
                bool running = true;
                while (running)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.R:
                            ToggleRecording();
                            break;
                        case ConsoleKey.P:
                            TogglePlayback();
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
                        case ConsoleKey.V:
                            ToggleAvatarVideoStreaming();
                            break;
                        case ConsoleKey.F:
                            StartFFplayForAvatarStreaming();
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
        ///     Audio input device for recording.
        /// </summary>
        private static WaveInEvent waveIn = null!;

        /// <summary>
        ///     Audio output device for playback.
        /// </summary>
        private static WaveOutEvent waveOut = null!;

        /// <summary>
        ///     Buffered wave provider for audio playback.
        /// </summary>
        private static BufferedWaveProvider waveProvider = null!;

        /// <summary>
        ///     Buffered wave provider for Avatar audio playback.
        /// </summary>
        private static BufferedWaveProvider? avatarWaveProvider;

        /// <summary>
        ///     Opus decoder for Avatar audio streams.
        /// </summary>
        private static IOpusDecoder? opusDecoder;

        /// <summary>
        ///     Flag indicating if recording is active.
        /// </summary>
        private static bool isRecording;

        /// <summary>
        ///     Flag indicating if playback is active.
        /// </summary>
        private static bool isPlaying;

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
        ///     Azure AI agent identifier for agent mode.
        /// </summary>
        private static string agentId = "<your Azure AI Agent Id>";

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
        ///     Avatar message handler manager for handling avatar-specific messages.
        /// </summary>
        private static AvatarMessageHandlerManager? avatarManager;

        /// <summary>
        ///     RTP-based avatar video streamer with synchronized A/V playback.
        /// </summary>
        private static AvatarVideoStreamer? avatarVideoStreamer;

        /// <summary>
        ///     Avatar client for WebRTC avatar video streaming.
        /// </summary>
        private static AvatarClient? avatarClient;

        /// <summary>
        ///     Current connection mode for reconnection purposes.
        /// </summary>
        private static ConnectionMode currentMode;

        /// <summary>
        ///     Foundry Agent name for tool calling (2026-01-01-preview).
        ///     When set, a FoundryAgentTool is added to session tools.
        /// </summary>
        private static string foundryAgentName = "";

        /// <summary>
        ///     Foundry Agent version for tool calling (2026-01-01-preview).
        /// </summary>
        private static string foundryAgentVersion = "";

        /// <summary>
        ///     Foundry Agent description for tool calling (2026-01-01-preview).
        ///     Required by the server for Foundry Agent tools.
        /// </summary>
        private static string foundryAgentDescription = "";

        /// <summary>
        ///     Indicates whether API Key authentication is used (false = EntraID/TokenCredential).
        /// </summary>
        private static bool useApiKeyAuth;

        /// <summary>
        ///     Audio queue for buffering audio data.
        /// </summary>
        private static readonly Queue<byte[]> AudioQueue = new();

        /// <summary>
        ///     Audio playback background task.
        /// </summary>
#pragma warning disable IDE0044 // Add readonly modifier
        private static Task audioPlaybackTask = Task.CompletedTask;
#pragma warning restore IDE0044 // Add readonly modifier

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
                case ConnectionMode.AIModelWithFoundryAgent:
                case ConnectionMode.AIAgent:
                    // Standard audio settings - match ClientSessionUpdate.Default
                    options.Avatar = null;
                    // Ensure modalities include audio for audio output
                    options.Modalities = new[] { "text", "audio" };
                    options.InputAudioFormat = "pcm16";
                    options.OutputAudioFormat = "pcm16";
                    options.InputAudioSamplingRate = 24000;
                    // Voice configuration - required for audio output
                    options.Voice = new Voice
                    {
                        Name = "ja-JP-Nanami:DragonHDLatestNeural",
                        Type = "azure-standard"
                    };
                    // Configure VAD with explicit settings to ensure response generation
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
                    // Input audio echo cancellation - prevents AI response audio from being
                    // picked up by the microphone and interrupting the response
                    options.InputAudioEchoCancellation = new AudioInputEchoCancellationSettings();
                    // Filler response - speaks filler phrases during latency or tool call wait times
                    options.FillerResponse = new BasicFillerResponseConfig
                    {
                        Texts = new[]
                        {
                            "少々お待ちください。",
                            "確認しますね。",
                            "ちょっと調べますね。",
                            "はい、少しお待ちくださいね。"
                        },
                        Triggers = new[] { "latency", "tool" },
                        LatencyThresholdMs = 2000
                    };

                    // Tool definitions and mode-specific settings
                    if (mode == ConnectionMode.AIModelWithFoundryAgent)
                    {
                        // Foundry Agent Tool - Preview API (2026-01-01-preview)
                        // Chat supervisor pattern: AI Model (gpt-4o) calls Foundry Agent as a tool.
                        // Output audio timestamp types for word-level timing
                        options.OutputAudioTimestampTypes = new[] { "word" };
                        // Animation settings for viseme output
                        options.Animation = new Animation
                        {
                            Outputs = new[] { "viseme_id" }
                        };

                        var foundryTool = new FoundryAgentTool
                        {
                            AgentName = foundryAgentName,
                            ProjectName = agentProjectName,
                            Description = foundryAgentDescription
                        };
                        if (!string.IsNullOrEmpty(foundryAgentVersion))
                        {
                            foundryTool.AgentVersion = foundryAgentVersion;
                        }

                        options.Tools = new RealtimeTool[] { foundryTool };
                        options.ToolChoice = "auto";
                        Console.WriteLine(
                            $"Foundry Agent Tool configured: agent_name={foundryAgentName}, project_name={agentProjectName}");
                    }
                    else if (mode == ConnectionMode.AIModel)
                    {
                        // Standard AI Model mode
                        // Output audio timestamp types for word-level timing
                        options.OutputAudioTimestampTypes = new[] { "word" };
                        // Animation settings for viseme output
                        options.Animation = new Animation
                        {
                            Outputs = new[] { "viseme_id" }
                        };

                        // Function Calling - サンプルツール定義
                        options.Tools = new RealtimeTool[]
                        {
                            new Function
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
                            }
                        };
                        options.ToolChoice = "auto";
                    }
                    else
                    {
                        // AI Agent mode - no custom tools (agent manages its own tools)
                        // Output audio timestamp types for word-level timing
                        options.OutputAudioTimestampTypes = new[] { "word" };
                        // Animation settings for viseme output
                        options.Animation = new Animation
                        {
                            Outputs = new[] { "viseme_id" }
                        };
                    }

                    break;

                case ConnectionMode.Avatar:
                    // Avatar mode - full settings like ClientSessionUpdate.Default
                    options.Modalities = new[] { "text", "audio" };
                    options.InputAudioFormat = "pcm16";
                    options.OutputAudioFormat = "pcm16";
                    options.InputAudioSamplingRate = 24000;
                    // Voice configuration
                    options.Voice = new Voice
                    {
                        Name = "ja-JP-Nanami:DragonHDLatestNeural",
                        Type = "azure-standard"
                    };
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
                    // Input audio echo cancellation
                    options.InputAudioEchoCancellation = new AudioInputEchoCancellationSettings();
                    // Avatar video settings
                    options.Avatar = new Avatar
                    {
                        Character = "lisa",
                        Style = "casual-sitting",
                        Customized = false,
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
            avatarManager = new AvatarMessageHandlerManager();
            voiceLiveSession.AddMessageHandlerManager(serverManager);

            if (currentMode == ConnectionMode.Avatar)
            {
                voiceLiveSession.AddMessageHandlerManager(avatarManager);
                logger?.LogInformation("Avatar mode: Added avatar message handler");

                // Initialize avatar client for Avatar mode
                avatarClient = new AvatarClient();
                logger?.LogInformation("Avatar client initialized for WebRTC streaming");
            }

            // Set up event handlers
            SetupServerManagerEvents();
            SetupAvatarManagerEvents();

            logger?.LogInformation("Session event handlers configured");
        }

        /// <summary>
        ///     Sets up ServerMessageHandlerManager events.
        /// </summary>
        private static void SetupServerManagerEvents()
        {
            if (serverManager == null) return;

            serverManager.OnAudioDeltaReceived += response =>
            {
                if (string.IsNullOrEmpty(response.Delta))
                {
                    logger?.LogWarning("Audio delta received but Delta is null or empty");
                    return;
                }

                byte[] pcmData = Convert.FromBase64String(response.Delta);

                if (currentMode == ConnectionMode.Avatar)
                {
                    // Avatar mode handles audio through WebRTC
                    return;
                }

                if (pcmData.Length > 0)
                {
                    // Guard against audio processing during shutdown
                    var provider = waveProvider;
                    var player = waveOut;
                    if (provider == null || player == null)
                    {
                        return;
                    }

                    // Add audio data directly to the wave provider for playback
                    lock (provider)
                    {
                        provider.AddSamples(pcmData, 0, pcmData.Length);
                    }

                    // Check actual playback state, not just the isPlaying flag
                    // NAudio may have stopped due to empty buffer even if isPlaying is true
                    try
                    {
                        if (player.PlaybackState != PlaybackState.Playing)
                        {
                            player.Play();
                            isPlaying = true;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // waveOut may have been disposed between the null check and Play() call
                    }
                }
            };

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

                if (sessionUpdate.Avatar == null || avatarClient == null || voiceLiveSession == null)
                {
                    StartRecording();
                    return;
                }

                if (sessionUpdate.Avatar.IceServers == null || sessionUpdate.Avatar.IceServers.Length == 0)
                {
                    logger?.LogWarning("Avatar is set but IceServers is null or empty");
                    StartRecording();
                    return;
                }

                try
                {
                    IceServers? ics = sessionUpdate.Avatar.IceServers[0];
                    logger?.LogInformation("Starting WebRTC connection with ICE servers: {urls}",
                        string.Join(", ", ics.Urls));

                    // Connect avatar client to WebRTC
                    await avatarClient.AvatarConnectAsync(ics, voiceLiveSession);

                    logger?.LogInformation("WebRTC connection initiated successfully");

                    // Initialize avatar video streaming
                    if (currentMode == ConnectionMode.Avatar && avatarVideoStreamer == null)
                    {
                        avatarVideoStreamer = new AvatarVideoStreamer(avatarClient,
                            logger ?? throw new NullReferenceException("logger is null"));

                        if (!avatarVideoStreamer.StartStreaming())
                        {
                            logger?.LogError("Failed to start avatar video streaming");
                            avatarVideoStreamer?.Dispose();
                            avatarVideoStreamer = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Exception in OnSessionUpdateReceived handler");
                    avatarVideoStreamer?.Dispose();
                    avatarVideoStreamer = null;
                }

                StartRecording();
            };

            serverManager.OnErrorReceived += response =>
            {
                logger?.LogError(
                    "Error received: code={Code}, message={Message}, error_type={ErrorType}, param={Param}",
                    response.Code, response.Message, response.ErrorType, response.Param);
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
            serverManager.OnConversationItemTruncatedReceived += DebugMessages;
            serverManager.OnInputAudioBufferClearedReceived += DebugMessages;
            serverManager.OnInputAudioBufferCommittedReceived += DebugMessages;
            serverManager.OnInputAudioBufferSpeechStartedReceived += DebugMessages;
            serverManager.OnInputAudioBufferSpeechStoppedReceived += DebugMessages;

            // Auto-stop recording when speech is detected as stopped
            serverManager.OnInputAudioBufferSpeechStoppedReceived += message =>
            {
                if (isRecording)
                {
                    logger?.LogTrace("🔇 Speech stopped detected (audio_end: {ms}ms) - auto-stopping recording",
                        message.AudioEndMs);
                    StopRecording();
                }
            };

            serverManager.OnOutputAudioBufferClearedReceived += DebugMessages;
            serverManager.OnOutputAudioBufferStartedReceived += DebugMessages;
            serverManager.OnOutputAudioBufferStoppedReceived += DebugMessages;
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

            // Foundry Agent call event handlers (2026-01-01-preview)
            serverManager.OnFoundryAgentCallArgumentsDeltaReceived += DebugMessages;
            serverManager.OnFoundryAgentCallArgumentsDoneReceived += done =>
            {
                Console.WriteLine("[Foundry Agent Call Arguments Done] item_id={0}, arguments={1}",
                    done.ItemId, done.Arguments);
            };
            serverManager.OnFoundryAgentCallInProgressReceived += inProgress =>
            {
                Console.WriteLine("[Foundry Agent Call In Progress] item_id={0}, agent_response_id={1}",
                    inProgress.ItemId, inProgress.AgentResponseId);
            };
            serverManager.OnFoundryAgentCallCompletedReceived += completed =>
            {
                Console.WriteLine("[Foundry Agent Call Completed] item_id={0}, agent_response_id={1}",
                    completed.ItemId, completed.AgentResponseId);
            };
            serverManager.OnFoundryAgentCallFailedReceived += failed =>
            {
                Console.WriteLine("[Foundry Agent Call Failed] item_id={0}, output_index={1}",
                    failed.ItemId, failed.OutputIndex);
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
        ///     Sets up AvatarMessageHandlerManager events.
        /// </summary>
        private static void SetupAvatarManagerEvents()
        {
            if (avatarManager == null) return;

            avatarManager.OnSessionAvatarConnecting += connecting =>
            {
                logger?.Log(LogLevel.Debug, "type : {Type}", connecting.Type);

                if (avatarClient == null)
                {
                    logger?.LogError("Avatar connecting event received but avatarClient is null");
                    return;
                }

                logger?.LogTrace("Setting remote SDP for WebRTC connection");
                avatarClient.AvatarConnecting(connecting.ServerSdp);
                logger?.LogTrace("Remote SDP set successfully");
            };
        }

        /// <summary>
        ///     Prompts the user to choose a connection mode.
        /// </summary>
        /// <returns>The selected connection mode.</returns>
        private static ConnectionMode ChooseConnectionMode()
        {
            Console.WriteLine("Choose connection mode:");
            Console.WriteLine("1. AI Model Mode");
            Console.WriteLine("2. AI Model + Foundry Agent Mode (Preview: 2026-01-01-preview)");
            Console.WriteLine("3. AI Agent Mode");
            Console.WriteLine("4. Avatar Mode (with video streaming)");
            Console.Write("Enter your choice (1, 2, 3, or 4): ");

            while (true)
            {
                try
                {
                    string? input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        Console.Write("Please enter 1, 2, 3, or 4: ");
                        continue;
                    }

                    switch (input.Trim())
                    {
                        case "1":
                            Console.WriteLine("Selected: AI Model Mode");
                            return ConnectionMode.AIModel;
                        case "2":
                            Console.WriteLine("Selected: AI Model + Foundry Agent Mode (Preview)");
                            return ConnectionMode.AIModelWithFoundryAgent;
                        case "3":
                            Console.WriteLine("Selected: AI Agent Mode");
                            return ConnectionMode.AIAgent;
                        case "4":
                            Console.WriteLine("Selected: Avatar Mode");
                            return ConnectionMode.Avatar;
                        default:
                            Console.Write("Invalid choice. Please enter 1, 2, 3, or 4: ");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error reading console input");
                    throw;
                }
            }
        }

        /// <summary>
        ///     Prompts the user for Foundry Agent tool configuration.
        ///     When agent name is provided, the AI Model session will include a FoundryAgentTool
        ///     (chat supervisor pattern) and use the preview API version (2026-01-01-preview).
        /// </summary>
        private static void PromptFoundryAgentToolConfig()
        {
            Console.WriteLine("\n--- Foundry Agent Tool Configuration (2026-01-01-preview) ---");
            Console.WriteLine("Configure a Foundry Agent as a callable tool for the chat supervisor pattern.");
            Console.WriteLine("The AI Model (gpt-4o) will act as supervisor and call the Foundry Agent as a tool.");
            Console.WriteLine("Leave blank to skip (standard AI Model mode with function calling).\n");

            Console.Write("Foundry Agent Name: ");
            foundryAgentName = Console.ReadLine()?.Trim() ?? "";

            if (!string.IsNullOrEmpty(foundryAgentName))
            {
                Console.Write("Agent Description (required): ");
                foundryAgentDescription = Console.ReadLine()?.Trim() ?? "";

                // Description is required by the server - use a default if empty
                if (string.IsNullOrEmpty(foundryAgentDescription))
                {
                    foundryAgentDescription = $"Foundry Agent tool for {foundryAgentName}";
                    Console.WriteLine($"  Using default description: \"{foundryAgentDescription}\"");
                }

                Console.Write("Agent Version (optional, press Enter to skip): ");
                foundryAgentVersion = Console.ReadLine()?.Trim() ?? "";

                Console.WriteLine(
                    $"\nFoundry Agent Tool will be configured: {foundryAgentName}" +
                    $" - \"{foundryAgentDescription}\"" +
                    (string.IsNullOrEmpty(foundryAgentVersion) ? "" : $" (v{foundryAgentVersion})"));
            }
            else
            {
                foundryAgentVersion = "";
                foundryAgentDescription = "";
                Console.WriteLine("No Foundry Agent Tool configured. Using standard AI Agent mode.");
            }
        }

        /// <summary>
        ///     Prints diagnostic information about the session configuration.
        ///     Outputs the session.update JSON and WebSocket URI for debugging.
        /// </summary>
        /// <param name="sessionOptions">The session options to diagnose.</param>
        private static void PrintSessionDiagnostics(VoiceLiveSessionOptions sessionOptions)
        {
            try
            {
                var diagnosticOptions = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true
                };

                var sessionUpdateMessage = new
                {
                    type = "session.update",
                    session = sessionOptions
                };

                var json = JsonSerializer.Serialize(sessionUpdateMessage, diagnosticOptions);
                Console.WriteLine("\n--- Diagnostic: session.update JSON ---");
                Console.WriteLine(json);
                Console.WriteLine("--- End of session.update JSON ---");

                // Print API version info
                if (voiceLiveClient?.Options != null)
                {
                    Console.WriteLine($"API Version: {voiceLiveClient.Options.ApiVersion}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Diagnostic] Failed to serialize session options: {ex.Message}");
            }
        }

        /// <summary>
        ///     Initializes the VoiceLive API client based on the specified mode.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static Task InitializeClientAsync()
        {
            Console.WriteLine("Choose authentication method:");
            Console.WriteLine("1. API Key");
            Console.WriteLine("2. Entra ID (DefaultAzureCredential)");
            Console.Write("Enter your choice (1 or 2): ");

            int authChoice = ChooseAuthMethod();
            useApiKeyAuth = authChoice == 1;

            try
            {
                // Use preview API version when Foundry Agent tool is configured
                VoiceLiveClientOptions clientOptions = null;
                if (!string.IsNullOrEmpty(foundryAgentName))
                {
                    clientOptions = new VoiceLiveClientOptions
                    {
                        ApiVersion = VoiceLiveClientOptions.PreviewApiVersion
                    };
                    Console.WriteLine($"Using Preview API version: {VoiceLiveClientOptions.PreviewApiVersion}");
                }

                if (useApiKeyAuth)
                {
                    // API Key authentication using AzureKeyCredential
                    logger?.LogInformation("Initializing VoiceLiveClient with API Key authentication...");
                    voiceLiveClient = new VoiceLiveClient(azureEndpoint, new AzureKeyCredential(apiKey),
                        clientOptions);
                }
                else
                {
                    // Entra ID authentication using DefaultAzureCredential
                    logger?.LogInformation("Initializing VoiceLiveClient with Entra ID authentication...");
                    voiceLiveClient = new VoiceLiveClient(
                        azureEndpoint,
                        new DefaultAzureCredential(),
                        new[] { azureIdentityTokenRequestUrl },
                        clientOptions);
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
        ///     Prompts the user to choose an authentication method.
        /// </summary>
        /// <returns>The selected authentication method (1 for API Key, 2 for Entra ID).</returns>
        private static int ChooseAuthMethod()
        {
            while (true)
            {
                try
                {
                    string? input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        Console.Write("Please enter 1 or 2: ");
                        continue;
                    }

                    switch (input.Trim())
                    {
                        case "1":
                            return 1;
                        case "2":
                            return 2;
                        default:
                            Console.Write("Invalid choice. Please enter 1 or 2: ");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error reading console input for authentication");
                    throw;
                }
            }
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
                StopRecording();
                StopPlayback();

                // Cleanup Avatar audio resources before switching
                CleanupAudio();

                // Cleanup avatar video streamer before switching
                if (avatarVideoStreamer != null)
                {
                    avatarVideoStreamer.StopStreaming();
                    avatarVideoStreamer.Dispose();
                    avatarVideoStreamer = null;
                }

                // Cleanup avatar client
                if (avatarClient != null)
                {
                    logger?.LogInformation("Cleaning up avatar client before mode switch");
                    avatarClient = null;
                }

                // Dispose current session
                if (voiceLiveSession != null)
                {
                    await voiceLiveSession.DisposeAsync();
                    voiceLiveSession = null;
                }

                // Choose new mode
                ConnectionMode newMode = ChooseConnectionMode();
                currentMode = newMode;

                // Prompt for Foundry Agent tool configuration (AI Model + Foundry Agent mode)
                if (currentMode == ConnectionMode.AIModelWithFoundryAgent)
                {
                    PromptFoundryAgentToolConfig();
                }

                // Initialize audio for new mode
                InitializeAudio();

                // Initialize new client
                await InitializeClientAsync();

                // Start new session
                Console.WriteLine($"Reconnecting in {newMode} mode...");
                VoiceLiveSessionOptions sessionOptions = CreateSessionOptions(newMode);

                // Diagnostic: Print the session.update JSON that will be sent
                PrintSessionDiagnostics(sessionOptions);

                if (newMode == ConnectionMode.AIModel || newMode == ConnectionMode.AIModelWithFoundryAgent)
                {
                    voiceLiveSession = await voiceLiveClient!.StartSessionAsync(sessionOptions);
                }
                else
                {
                    voiceLiveSession = await voiceLiveClient!.StartAgentSessionAsync(
                        agentProjectName, agentId, sessionOptions);
                }

                SetupSessionEventHandlers();
                StartRecording();

                Console.WriteLine("Mode switched successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error switching mode: {ex.Message}");
            }
        }

        /// <summary>
        ///     Initializes audio input and output components.
        /// </summary>
        private static void InitializeAudio()
        {
            // Setup audio input (microphone) - same for all modes
            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(SampleRate, BitsPerSample, Channels),
                BufferMilliseconds = 100
            };
            waveIn.DataAvailable += OnAudioDataAvailable!;
            waveIn.RecordingStopped += OnRecordingStopped!;

            // Setup audio output (speakers)
            waveOut = new WaveOutEvent();

            // Initialize regular audio provider (24kHz, mono, 16-bit)
            // Buffer must be large enough to hold audio that arrives faster than real-time.
            // Server TTS sends audio at ~2.5x real-time, so for a 120-second response,
            // up to ~72 seconds of audio may accumulate in the buffer.
            waveProvider = new BufferedWaveProvider(new WaveFormat(SampleRate, BitsPerSample, Channels))
            {
                BufferLength = SampleRate * Channels * 2 * 180, // 180 seconds buffer (~8.6MB)
                DiscardOnBufferOverflow = true,
                ReadFully = true
            };

            // Initialize Avatar audio provider if in Avatar mode (48kHz, stereo, 16-bit)
            if (currentMode == ConnectionMode.Avatar)
            {
                avatarWaveProvider =
                    new BufferedWaveProvider(new WaveFormat(AvatarSampleRate, BitsPerSample, AvatarChannels))
                    {
                        BufferLength = AvatarSampleRate * AvatarChannels * 2 * 10, // 10 seconds buffer
                        DiscardOnBufferOverflow = true
                    };

                // Initialize Opus decoder for Avatar audio
                try
                {
                    opusDecoder = OpusCodecFactory.CreateDecoder(AvatarSampleRate, AvatarChannels);
                    logger?.LogInformation(
                        "Opus decoder initialized for Avatar mode: {sampleRate}Hz, {channels} channels",
                        AvatarSampleRate, AvatarChannels);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to initialize Opus decoder for Avatar mode");
                    throw;
                }

                // Use Avatar wave provider for output
                waveOut.Init(avatarWaveProvider);

                logger?.LogInformation("Audio initialized for Avatar mode: {sampleRate}Hz, {channels} channels",
                    AvatarSampleRate, AvatarChannels);
            }
            else
            {
                // Use regular wave provider for output
                waveOut.Init(waveProvider);

                logger?.LogInformation("Audio initialized for regular mode: {sampleRate}Hz, {channels} channel",
                    SampleRate,
                    Channels);
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

            string output;

            switch (functionCallDone.Name)
            {
                case "get_weather":
                    output = HandleGetWeather(functionCallDone.Arguments);
                    break;
                default:
                    logger?.LogWarning("Unknown function: {Name}", functionCallDone.Name);
                    output = JsonSerializer.Serialize(new { error = $"Unknown function: {functionCallDone.Name}" });
                    break;
            }

            Console.WriteLine("[Function Call] {0}({1}) => {2}", functionCallDone.Name, functionCallDone.Arguments,
                output);

            // Send the function call output back to the server
            await voiceLiveSession.SendFunctionCallOutputAsync(functionCallDone.CallId, output);

            // Trigger a new response to have the model respond with the function result
            await voiceLiveSession.CreateResponseAsync();

            logger?.LogInformation("Function call output sent and response triggered for call_id={CallId}",
                functionCallDone.CallId);
        }

        /// <summary>
        ///     Sample implementation of the get_weather function.
        /// </summary>
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
            if (currentMode != ConnectionMode.AIModel && currentMode != ConnectionMode.AIModelWithFoundryAgent)
            {
                Console.WriteLine("Image input is only supported in AI Model mode.");
                return;
            }

            if (voiceLiveSession == null)
            {
                Console.WriteLine("Session is not initialized.");
                return;
            }

            // Stop recording temporarily to allow console input
            bool wasRecording = isRecording;
            if (wasRecording)
            {
                StopRecording();
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
                    StartRecording();
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
                StartRecording();
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

        private static async void OnAudioDataAvailable(object? sender, WaveInEventArgs e)
        {
            // Capture in local variable to avoid TOCTOU race condition
            var session = voiceLiveSession;
            if (!isRecording || e.BytesRecorded <= 0 || session == null) return;

            try
            {
                byte[] audioData = new byte[e.BytesRecorded];
                Array.Copy(e.Buffer, 0, audioData, 0, e.BytesRecorded);

                await session.SendInputAudioAsync(audioData);
            }
            catch (Exception ex)
            {
                logger?.LogError("Error sending audio data: {Message}", ex.Message);
            }
        }

        private static void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            logger?.LogTrace("Recording stopped");
            if (e.Exception != null)
            {
                logger?.LogError("Recording error: {Message}", e.Exception.Message);
            }
        }

        private static void StartRecording()
        {
            if (!isRecording)
            {
                try
                {
                    Console.WriteLine("Starting microphone...");
                    waveIn.StartRecording();
                    isRecording = true;
                    Console.WriteLine(
                        "🎤 Recording Start - Stops automatically when you finish speaking (Manual stop:'R' key)");
                }
                catch (Exception ex)
                {
                    logger?.LogError("Error starting recording: {Message}", ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Already recording");
            }
        }

        private static void StopRecording()
        {
            if (isRecording)
            {
                try
                {
                    waveIn.StopRecording();
                    isRecording = false;
                    Console.WriteLine("Recording stopped");
                }
                catch (Exception ex)
                {
                    logger?.LogError("Error stopping recording: {Message}", ex.Message);
                }
            }
        }

        private static void StartPlayback()
        {
            if (waveOut.PlaybackState != PlaybackState.Playing)
            {
                try
                {
                    waveOut.Play();
                    isPlaying = true;
                    Console.WriteLine("Playback started");
                }
                catch (Exception ex)
                {
                    logger?.LogError("Error starting playback: {Message}", ex.Message);
                }
            }
        }

        private static void StopPlayback()
        {
            if (waveOut.PlaybackState == PlaybackState.Playing)
            {
                try
                {
                    waveOut.Stop();
                    isPlaying = false;
                    Console.WriteLine("Playback stopped");
                }
                catch (Exception ex)
                {
                    logger?.LogError("Error stopping playback: {Message}", ex.Message);
                }
            }

            isPlaying = false; // Always reset the flag
        }

        /// <summary>
        ///     Toggles audio recording on or off.
        /// </summary>
        private static void ToggleRecording()
        {
            if (isRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        /// <summary>
        ///     Toggles audio playback on or off.
        /// </summary>
        private static void TogglePlayback()
        {
            if (isPlaying)
            {
                StopPlayback();
            }
            else
            {
                StartPlayback();
            }
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
            Console.WriteLine($"Recording: {(isRecording ? "ON" : "OFF")}");
            Console.WriteLine($"Playback: {(isPlaying ? "ON" : "OFF")}");
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

            if (waveProvider != null)
            {
                TimeSpan bufferedDuration = waveProvider.BufferedDuration;
                Console.WriteLine($"Buffer Duration: {bufferedDuration.TotalSeconds:F2} seconds");
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

            StopRecording();
            StopPlayback();

            waveIn?.Dispose();
            waveOut?.Dispose();
            waveProvider = null!;

            // Cleanup Avatar audio resources
            CleanupAudio();

            // Cleanup avatar video streaming
            if (avatarVideoStreamer != null)
            {
                avatarVideoStreamer.StopStreaming();
                avatarVideoStreamer.Dispose();
                avatarVideoStreamer = null;
            }

            // Cleanup avatar client
            if (avatarClient != null)
            {
                logger?.LogInformation("Cleaning up avatar client");
                avatarClient = null;
            }

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
        ///     Toggles avatar video streaming.
        /// </summary>
        private static void ToggleAvatarVideoStreaming()
        {
            try
            {
                if (currentMode != ConnectionMode.Avatar)
                {
                    Console.WriteLine("⚠️ Video streaming is only available in Avatar mode");
                    return;
                }

                if (avatarVideoStreamer == null)
                {
                    Console.WriteLine("⚠️ Avatar video streamer not initialized. Connect to avatar first.");
                    return;
                }

                Console.WriteLine("🎥 Avatar RTP streaming is active");
                Console.WriteLine("   - Status: Real-time synchronized audio/video playback");
                Console.WriteLine("   - Video window opens automatically when streaming starts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error toggling video streaming: {ex.Message}");
                logger?.LogError(ex, "Error toggling avatar video streaming");
            }
        }

        /// <summary>
        ///     Shows information about avatar streaming (file output removed for performance).
        /// </summary>
        private static void StartFFplayForAvatarStreaming()
        {
            try
            {
                if (currentMode != ConnectionMode.Avatar)
                {
                    Console.WriteLine("⚠️ Avatar streaming is only available in Avatar mode");
                    return;
                }

                if (avatarVideoStreamer == null)
                {
                    Console.WriteLine("⚠️ Avatar video streamer not initialized. Connect to avatar first.");
                    return;
                }

                Console.WriteLine("ℹ️ Avatar streaming information:");
                Console.WriteLine("   - Real-time RTP streaming is active");
                Console.WriteLine("   - Video window opens automatically when streaming starts");
                Console.WriteLine("   - File output has been removed for performance optimization");
                Console.WriteLine("   - All playback is real-time only");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error showing streaming information: {ex.Message}");
                logger?.LogError(ex, "Error showing avatar streaming information");
            }
        }

        /// <summary>
        ///     Starts audio playback if not already playing.
        /// </summary>
        private static void StartAudioPlayback()
        {
            if (waveOut != null && waveOut.PlaybackState != PlaybackState.Playing)
            {
                try
                {
                    waveOut.Play();
                    isPlaying = true;
                    logger?.LogInformation("Audio playback started");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error starting audio playback");
                }
            }
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
                StopRecording();
                StopPlayback();

                // Cleanup Avatar audio resources before reconnection
                CleanupAudio();

                // Cleanup avatar video streaming before reconnection
                if (avatarVideoStreamer != null)
                {
                    avatarVideoStreamer.StopStreaming();
                    avatarVideoStreamer.Dispose();
                    avatarVideoStreamer = null;
                }

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

                // Diagnostic: Print the session.update JSON that will be sent
                PrintSessionDiagnostics(sessionOptions);

                if (currentMode == ConnectionMode.AIModel || currentMode == ConnectionMode.AIModelWithFoundryAgent)
                {
                    voiceLiveSession = await voiceLiveClient!.StartSessionAsync(sessionOptions);
                }
                else
                {
                    voiceLiveSession = await voiceLiveClient!.StartAgentSessionAsync(
                        agentProjectName, agentId, sessionOptions);
                }

                SetupSessionEventHandlers();

                // Restart recording
                StartRecording();

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

                // Use preview API version when Foundry Agent tool is configured
                VoiceLiveClientOptions clientOptions = null;
                if (!string.IsNullOrEmpty(foundryAgentName))
                {
                    clientOptions = new VoiceLiveClientOptions
                    {
                        ApiVersion = VoiceLiveClientOptions.PreviewApiVersion
                    };
                }

                if (useApiKeyAuth)
                {
                    voiceLiveClient = new VoiceLiveClient(azureEndpoint, new AzureKeyCredential(apiKey),
                        clientOptions);
                }
                else
                {
                    voiceLiveClient = new VoiceLiveClient(
                        azureEndpoint,
                        new DefaultAzureCredential(),
                        new[] { azureIdentityTokenRequestUrl },
                        clientOptions);
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

        /// <summary>
        ///     Cleans up Avatar audio resources by properly disposing of avatarWaveProvider and opusDecoder.
        /// </summary>
        private static void CleanupAudio()
        {
            try
            {
                // Dispose of Avatar wave provider
                if (avatarWaveProvider != null)
                {
                    try
                    {
                        avatarWaveProvider.ClearBuffer();
                        logger?.LogInformation("Avatar wave provider cleared and disposed");
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Error clearing avatar wave provider buffer");
                    }
                    finally
                    {
                        avatarWaveProvider = null;
                    }
                }

                // Reset Opus decoder reference
                if (opusDecoder != null)
                {
                    opusDecoder = null;
                    logger?.LogInformation("Opus decoder reset");
                }

                Console.WriteLine("🧹 Avatar audio resources cleaned up");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during avatar audio cleanup");
                Console.WriteLine($"⚠️ Warning: Error cleaning up avatar audio resources: {ex.Message}");
            }
        }

        #endregion
    }
}