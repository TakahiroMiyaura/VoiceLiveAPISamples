// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text;
using Azure;
using Azure.AI.VoiceLive;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveSDK
{
    /// <summary>
    ///     Specifies the connection mode for the VoiceLive SDK client.
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
        Avatar
    }

    /// <summary>
    ///     Main console application class for the VoiceLive SDK sample application.
    ///     Demonstrates usage of the official Azure.AI.VoiceLive SDK package.
    /// </summary>
    internal class Program
    {
        #region Static Fields and Constants

        private static ILogger? logger;
        private static IDisposable? telemetryListener;
        private static string azureEndpoint = "<your Azure AI Services Endpoint>";
        private static string agentProjectName = "<your Azure AI Foundry Project Name>";
        private static string agentName = "<your Azure AI Agent Name>";
        private static string agentId = "<your Azure AI Agent Id>";
        private static string voiceName = "ja-JP-Nanami:DragonHDLatestNeural";
        private static string modelName = "gpt-4o";
        private static string avatarBackend = "agent";

        /// <summary>OpenAI native voice names (used with GPT real-time models), e.g. "marin" / "cedar".</summary>
        private static readonly HashSet<string> OpenAiVoiceNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "alloy", "ash", "ballad", "coral", "echo", "sage", "shimmer", "verse", "marin", "cedar"
        };
        private static string azureIdentityTokenRequestUrl = "<Token request url(ex:https://ai.azure.com/.default)>";
        private static string apiKey = "<Azure AI Foundry API Key>";
        private static string agentAccessToken = "<Azure AI Foundry API Key>";

        private static ConnectionMode currentMode;
        private static bool useApiKeyAuth;

        private static VoiceLiveClient? voiceLiveClient;
        private static VoiceLiveAssistant? assistant;
        private static AudioHandler? audioHandler;
        private static AvatarHandler? avatarHandler;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Main entry point of the console application.
        /// </summary>
        [STAThread]
        private static async Task Main()
        {
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

            logger = loggerFactory.CreateLogger<Program>();

            // Subscribe to the SDK's OpenTelemetry tracing (beta.4) to surface token usage / latency.
            telemetryListener = VoiceLiveTelemetry.Enable(logger);

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            azureIdentityTokenRequestUrl = config["Identity:AzureEndpoint"] ?? azureIdentityTokenRequestUrl;
            azureEndpoint = config["VoiceLiveAPI:AzureEndpoint"] ?? azureEndpoint;
            apiKey = config["AzureAIFoundry:ApiKey"] ?? apiKey;
            agentProjectName = config["AzureAIFoundry:AgentProjectName"] ?? agentProjectName;
            agentName = config["AzureAIFoundry:AgentName"] ?? agentName;
            agentId = config["AzureAIFoundry:AgentId"] ?? agentId;
            voiceName = config["VoiceLiveAPI:Voice"] ?? voiceName;
            agentAccessToken = config["AzureAIFoundry:AgentAccessToken"] ?? agentAccessToken;

            // Environment-variable overrides (shared with the integration tests) take precedence over
            // user-secrets, so the same env setup used for tests also works for the console.
            modelName = config["VoiceLiveAPI:Model"] ?? modelName;
            azureEndpoint = Environment.GetEnvironmentVariable("VOICELIVE_ENDPOINT") ?? azureEndpoint;
            apiKey = Environment.GetEnvironmentVariable("VOICELIVE_APIKEY") ?? apiKey;
            agentName = Environment.GetEnvironmentVariable("VOICELIVE_AGENT_NAME") ?? agentName;
            agentProjectName = Environment.GetEnvironmentVariable("VOICELIVE_AGENT_PROJECT") ?? agentProjectName;
            voiceName = Environment.GetEnvironmentVariable("VOICELIVE_VOICE") ?? voiceName;
            modelName = Environment.GetEnvironmentVariable("VOICELIVE_MODEL") ?? modelName;
            avatarBackend = Environment.GetEnvironmentVariable("VOICELIVE_AVATAR_BACKEND") ?? avatarBackend;

            if (string.IsNullOrWhiteSpace(azureEndpoint) || !Uri.IsWellFormedUriString(azureEndpoint, UriKind.Absolute))
            {
                Console.WriteLine("[Config] Voice Live endpoint is not configured.");
                Console.WriteLine("  Set env VOICELIVE_ENDPOINT, or user-secret 'VoiceLiveAPI:AzureEndpoint'");
                Console.WriteLine("  e.g. https://<your-resource>.cognitiveservices.azure.com");
                return;
            }

            Console.WriteLine("Azure VoiceLive SDK Console Application");
            Console.WriteLine("Using Azure.AI.VoiceLive SDK (Official Azure SDK)");
            Console.WriteLine("================================================");

            try
            {
                currentMode = ChooseConnectionMode();
                InitializeClient();

                audioHandler = new AudioHandler(logger);
                audioHandler.Initialize(currentMode == ConnectionMode.Avatar);

                if (currentMode == ConnectionMode.Avatar)
                {
                    avatarHandler = new AvatarHandler(logger);
                    avatarHandler.Initialize();
                }

                Console.WriteLine($"Connecting to Azure VoiceLive API in {currentMode} mode...");

                var (model, sessionOptions) = CreateSessionOptions(currentMode);

                assistant = new VoiceLiveAssistant(
                    voiceLiveClient!,
                    audioHandler,
                    avatarHandler,
                    currentMode,
                    logger);

                await assistant.StartAsync(
                    BuildSessionTarget(currentMode, model),
                    sessionOptions);

                Console.WriteLine("\nReady for conversation!");
                PrintCommands();

                bool running = true;
                while (running)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.R:
                            audioHandler.ToggleRecording();
                            break;
                        case ConsoleKey.P:
                            audioHandler.TogglePlayback();
                            break;
                        case ConsoleKey.M:
                            await SwitchModeAsync();
                            break;
                        case ConsoleKey.I:
                            await SendImageAsync();
                            break;
                        case ConsoleKey.C:
                            await ClearAudioAsync();
                            break;
                        case ConsoleKey.S:
                            ShowStatus();
                            break;
                        case ConsoleKey.T:
                            await TestAndReconnectAsync();
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
                await CleanupAsync();
            }
        }

        #endregion

        #region Private Methods

        private static void PrintCommands()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("- 'R' record (auto-stops when you finish speaking)");
            Console.WriteLine("- 'I' send an image (vision-capable model)");
            Console.WriteLine("- 'Q' quit");
            Console.WriteLine("  (diagnostics: 'S' status, 'C' clear audio, 'P' playback, 'T' reconnect, 'M' switch mode)");
            Console.WriteLine();
            Console.WriteLine("  The avatar video window opens automatically once frames arrive (FFplay must be on PATH).");
        }

        private static (string model, VoiceLiveSessionOptions options) CreateSessionOptions(ConnectionMode mode)
        {
            string model = modelName;

            var options = new VoiceLiveSessionOptions
            {
                InputAudioFormat = InputAudioFormat.Pcm16,
                OutputAudioFormat = OutputAudioFormat.Pcm16,
                Voice = CreateVoice(voiceName),
                TurnDetection = new ServerVadTurnDetection
                {
                    Threshold = 0.5f,
                    SilenceDuration = TimeSpan.FromMilliseconds(500),
                    PrefixPadding = TimeSpan.FromMilliseconds(300)
                },
                InputAudioEchoCancellation = new AudioEchoCancellation()
            };

            // 'instructions' is not supported for custom agent sessions; only set it for AI Model mode.
            if (mode == ConnectionMode.AIModel)
            {
                options.Instructions = "You are a helpful AI assistant. Please respond in the same language as the user speaks.";
            }

            options.Modalities.Clear();
            options.Modalities.Add(InteractionModality.Text);
            options.Modalities.Add(InteractionModality.Audio);

            // Function Calling - sample tool definition (AI Model mode only)
            if (mode == ConnectionMode.AIModel)
            {
                options.Tools.Add(new VoiceLiveFunctionDefinition("get_weather")
                {
                    Description = "Get the current weather for a given location. The user may ask in any language.",
                    Parameters = BinaryData.FromObjectAsJson(new
                    {
                        type = "object",
                        properties = new
                        {
                            location = new
                            {
                                type = "string",
                                description = "The city and country, e.g. 'Tokyo, Japan'"
                            },
                            unit = new
                            {
                                type = "string",
                                @enum = new[] { "celsius", "fahrenheit" },
                                description = "Temperature unit"
                            }
                        },
                        required = new[] { "location" }
                    })
                });
            }

            // Avatar mode - configure avatar character and video settings
            if (mode == ConnectionMode.Avatar)
            {
                options.Avatar = new AvatarConfiguration("lisa", false)
                {
                    Style = "casual-sitting",
                    Video = new VideoParams
                    {
                        Bitrate = 2000000,
                        Codec = "h264",
                        Crop = new VideoCrop(
                            new int[] { 560, 0 },
                            new int[] { 1360, 1080 }),
                        Resolution = new VideoResolution(1920, 1080),
                        Background = new VideoBackground { Color = "#FFFFFFFF" }
                    }
                };
            }

            return (model, options);
        }

        private static ConnectionMode ChooseConnectionMode()
        {
            Console.WriteLine("Choose connection mode:");
            Console.WriteLine("1. AI Model Mode");
            Console.WriteLine("2. AI Agent Mode");
            Console.WriteLine("3. Avatar Mode (with video streaming)");
            Console.Write("Enter your choice (1, 2, or 3): ");

            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.Write("Please enter 1, 2, or 3: ");
                    continue;
                }

                switch (input.Trim())
                {
                    case "1":
                        Console.WriteLine("Selected: AI Model Mode");
                        return ConnectionMode.AIModel;
                    case "2":
                        Console.WriteLine("Selected: AI Agent Mode");
                        return ConnectionMode.AIAgent;
                    case "3":
                        Console.WriteLine("Selected: Avatar Mode");
                        ChooseAvatarBackend();
                        return ConnectionMode.Avatar;
                    default:
                        Console.Write("Invalid choice. Please enter 1, 2, or 3: ");
                        break;
                }
            }
        }

        /// <summary>
        ///     Prompts for the session backend used underneath Avatar output and stores it in
        ///     <see cref="avatarBackend" />. Agent (default) manages the conversation server-side; Model runs
        ///     on a direct model session, which enables model-only features such as image input.
        ///     The environment variable <c>VOICELIVE_AVATAR_BACKEND</c> supplies the default selection.
        /// </summary>
        private static void ChooseAvatarBackend()
        {
            bool defaultIsModel = string.Equals(avatarBackend, "model", StringComparison.OrdinalIgnoreCase);

            Console.WriteLine("Choose Avatar session backend:");
            Console.WriteLine("1. Agent (Foundry agent, Entra ID required)");
            Console.WriteLine("2. Model (direct model session, enables image input)");
            Console.Write($"Enter your choice (1 or 2) [default: {(defaultIsModel ? "2" : "1")}]: ");

            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                switch (input.Trim())
                {
                    case "1":
                        avatarBackend = "agent";
                        break;
                    case "2":
                        avatarBackend = "model";
                        break;
                    default:
                        Console.Write("Invalid choice. Please enter 1 or 2: ");
                        continue;
                }

                break;
            }

            Console.WriteLine($"Avatar backend: {(string.Equals(avatarBackend, "model", StringComparison.OrdinalIgnoreCase) ? "Model" : "Agent")}");
        }

        private static void InitializeClient()
        {
            if (!IsAgentSession(currentMode))
            {
                Console.WriteLine("Choose authentication method:");
                Console.WriteLine("1. API Key");
                Console.WriteLine("2. Entra ID (DefaultAzureCredential)");
                Console.Write("Enter your choice (1 or 2): ");

                useApiKeyAuth = ChooseAuthMethod() == 1;
            }
            else
            {
                // Foundry agent invocation does not support key-based authentication; force Entra ID.
                useApiKeyAuth = false;
                Console.WriteLine("Agent mode requires Entra ID authentication (API key is not supported for agent invocation).");
                Console.WriteLine("Using DefaultAzureCredential (sign in with 'az login' and ensure the appropriate Foundry RBAC role).");
            }

            voiceLiveClient = CreateVoiceLiveClient(currentMode);

            logger?.LogInformation("VoiceLiveClient (SDK) initialized successfully");
        }

        /// <summary>
        ///     Builds the Entra ID credential used for keyless auth.
        /// </summary>
        /// <remarks>
        ///     Managed identity is excluded because this runs on a developer machine, where there is none:
        ///     leaving it in makes the chain probe the IMDS endpoint and wait for it to time out, which looks
        ///     like the app hanging right after "Connecting...".
        /// </remarks>
        /// <returns>The credential.</returns>
        private static DefaultAzureCredential CreateCredential()
        {
            return new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeManagedIdentityCredential = true
            });
        }

        /// <summary>
        ///     Creates a <see cref="VoiceLiveClient" /> configured with the service (wire API) version
        ///     appropriate for the given mode: GA (2025-10-01) for AI Model, and 2026-01-01-preview for
        ///     AI Agent / Avatar (Foundry agent integration requires the preview wire API). The
        ///     ServiceVersion is fixed at client construction, so the client is (re)created whenever the
        ///     mode is selected or switched.
        /// </summary>
        /// <remarks>
        ///     SDK 1.2.0 added two GA wire versions, <c>2026-04-10</c> and <c>2026-07-15</c>, and much of what
        ///     used to be preview-only ships in them. The defaults above are unchanged so behavior stays the
        ///     same, but <c>VOICELIVE_SDK_SERVICE_VERSION</c> selects another one — pass the enum name
        ///     (<c>V2026_07_15</c>) or the wire version (<c>2026-07-15</c>).
        /// </remarks>
        /// <param name="mode">The connection mode the client will be used for.</param>
        /// <returns>A configured <see cref="VoiceLiveClient" /> instance.</returns>
        private static VoiceLiveClient CreateVoiceLiveClient(ConnectionMode mode)
        {
            VoiceLiveClientOptions.ServiceVersion serviceVersion = mode == ConnectionMode.AIModel
                ? VoiceLiveClientOptions.ServiceVersion.V2025_10_01
                : VoiceLiveClientOptions.ServiceVersion.V2026_01_01_PREVIEW;

            string? requested = Environment.GetEnvironmentVariable("VOICELIVE_SDK_SERVICE_VERSION");
            if (!string.IsNullOrWhiteSpace(requested))
            {
                string wanted = requested.Trim().Replace("-", "_");
                if (!wanted.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                {
                    wanted = "V" + wanted;
                }

                if (Enum.TryParse(wanted, true, out VoiceLiveClientOptions.ServiceVersion parsed))
                {
                    serviceVersion = parsed;
                }
                else
                {
                    Console.WriteLine($"Unknown service version '{requested}'. Using {serviceVersion}. "
                                      + $"Available: {string.Join(", ", Enum.GetNames<VoiceLiveClientOptions.ServiceVersion>())}");
                }
            }

            VoiceLiveClientOptions clientOptions = new VoiceLiveClientOptions(serviceVersion);
            Uri endpoint = new Uri(azureEndpoint);

            logger?.LogInformation(
                "Initializing VoiceLiveClient (SDK) - mode={mode}, serviceVersion={version}, auth={auth}",
                mode, serviceVersion, useApiKeyAuth ? "API Key" : "Entra ID");

            return useApiKeyAuth
                ? new VoiceLiveClient(endpoint, new AzureKeyCredential(apiKey), clientOptions)
                : new VoiceLiveClient(endpoint, CreateCredential(), clientOptions);
        }

        /// <summary>
        ///     Builds the session target for the given mode: a model session for AI Model, or a Foundry
        ///     agent session (new agent-name method via <see cref="AgentSessionConfig" />) for AI Agent /
        ///     Avatar.
        /// </summary>
        /// <param name="mode">The connection mode.</param>
        /// <param name="model">The AI model name (used for AI Model mode).</param>
        /// <returns>A <see cref="SessionTarget" /> describing the session to start.</returns>
        private static SessionTarget BuildSessionTarget(ConnectionMode mode, string model)
        {
            if (!IsAgentSession(mode))
            {
                return SessionTarget.FromModel(model);
            }

            AgentSessionConfig agentConfig = new AgentSessionConfig(agentName, agentProjectName);
            return SessionTarget.FromAgent(agentConfig);
        }

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
                   || (mode == ConnectionMode.Avatar &&
                       !string.Equals(avatarBackend, "model", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Builds the session voice from a configured name. OpenAI native voices (e.g. "marin",
        ///     "cedar") are mapped to <see cref="OpenAIVoice" /> — they require a GPT real-time
        ///     native-audio model; any other value is treated as an Azure standard/custom voice name
        ///     (e.g. "ja-JP-Nanami:DragonHDLatestNeural") via <see cref="AzureStandardVoice" />.
        /// </summary>
        /// <param name="voice">The configured voice name (<c>VoiceLiveAPI:Voice</c>).</param>
        /// <returns>A <see cref="VoiceProvider" /> for the session options.</returns>
        private static VoiceProvider CreateVoice(string voice)
        {
            if (OpenAiVoiceNames.Contains(voice))
            {
                return new OpenAIVoice(new OAIVoice(voice));
            }

            return new AzureStandardVoice(voice);
        }

        private static int ChooseAuthMethod()
        {
            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.Write("Please enter 1 or 2: ");
                    continue;
                }

                switch (input.Trim())
                {
                    case "1": return 1;
                    case "2": return 2;
                    default:
                        Console.Write("Invalid choice. Please enter 1 or 2: ");
                        break;
                }
            }
        }

        private static async Task SwitchModeAsync()
        {
            try
            {
                Console.WriteLine("\nSwitching mode...");

                // Cleanup current session
                audioHandler?.StopRecording();

                if (assistant != null)
                {
                    await assistant.DisposeAsync();
                    assistant = null;
                }

                avatarHandler?.Dispose();
                avatarHandler = null;

                audioHandler?.Dispose();
                audioHandler = null;

                // Choose new mode
                currentMode = ChooseConnectionMode();
                InitializeClient();

                // Reinitialize
                audioHandler = new AudioHandler(logger!);
                audioHandler.Initialize(currentMode == ConnectionMode.Avatar);

                if (currentMode == ConnectionMode.Avatar)
                {
                    avatarHandler = new AvatarHandler(logger!);
                    avatarHandler.Initialize();
                }

                Console.WriteLine($"Reconnecting in {currentMode} mode...");
                var (model, sessionOptions) = CreateSessionOptions(currentMode);

                assistant = new VoiceLiveAssistant(
                    voiceLiveClient!,
                    audioHandler,
                    avatarHandler,
                    currentMode,
                    logger!);

                await assistant.StartAsync(
                    BuildSessionTarget(currentMode, model),
                    sessionOptions);

                Console.WriteLine("Mode switched successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error switching mode: {ex.Message}");
            }
        }

        // Max image file size for the 'I' command. A large base64 data URI can exceed the Voice Live
        // WebSocket message limit and cause the server to drop the connection, so oversized images are
        // rejected up front (resize/compress the image below this size before sending).
        private const long MaxImageSizeBytes = 256 * 1024;

        private static async Task SendImageAsync()
        {
            if (assistant?.IsConnected != true)
            {
                Console.WriteLine("Not connected. Start a session first.");
                return;
            }

            Console.Write($"Image path (press Enter for the bundled sample; max {MaxImageSizeBytes / 1024} KB): ");
            string? path = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Combine(AppContext.BaseDirectory, "Assets", "sample_geometric.png");
            }

            path = path.Trim('"');
            if (!File.Exists(path))
            {
                Console.WriteLine($"Image not found: {path}");
                return;
            }

            long size = new FileInfo(path).Length;
            if (size > MaxImageSizeBytes)
            {
                Console.WriteLine($"Image too large: {size / 1024} KB (max {MaxImageSizeBytes / 1024} KB). Not sent.");
                Console.WriteLine("  Resize/compress the image (e.g. <=1024px) and try again.");
                return;
            }

            Console.WriteLine($"Sending image: {path} ({size / 1024} KB)");
            try
            {
                await assistant!.SendImageAsync(path);
                Console.WriteLine("Image sent. The model's description will follow in the response (and avatar, if in Avatar mode).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send image: {ex.Message}");
            }
        }

        private static async Task ClearAudioAsync()
        {
            if (assistant != null)
            {
                Console.WriteLine("Clearing audio queue...");
                await assistant.ClearStreamingAudioAsync();
                Console.WriteLine("Audio queue cleared");
            }
            else
            {
                Console.WriteLine("Session not initialized");
            }
        }

        private static void ShowStatus()
        {
            Console.WriteLine("\n=== Current Status (SDK) ===");
            Console.WriteLine($"Recording: {(audioHandler?.IsRecording == true ? "ON" : "OFF")}");
            Console.WriteLine($"Playback: {(audioHandler?.IsPlaying == true ? "ON" : "OFF")}");
            Console.WriteLine($"Connection Mode: {currentMode}");
            Console.WriteLine($"Auth Method: {(useApiKeyAuth ? "API Key" : "Entra ID")}");
            Console.WriteLine($"SDK Package: Azure.AI.VoiceLive");
            Console.WriteLine($"Connected: {(assistant?.IsConnected == true ? "Yes" : "No")}");
            Console.WriteLine($"Endpoint: {azureEndpoint}");

            if (audioHandler != null)
            {
                Console.WriteLine($"Buffer Duration: {audioHandler.GetBufferedDuration().TotalSeconds:F2} seconds");
            }

            if (avatarHandler != null)
            {
                Console.WriteLine($"Avatar Initialized: {avatarHandler.IsInitialized}");
                Console.WriteLine($"Avatar Streaming: {avatarHandler.IsStreaming}");
            }

            Console.WriteLine("============================\n");
        }

        private static async Task TestAndReconnectAsync()
        {
            try
            {
                Console.WriteLine("\nTesting connection...");

                if (assistant?.IsConnected == true)
                {
                    Console.WriteLine("Connection is healthy");
                    return;
                }

                Console.WriteLine("Connection issues detected, attempting reconnection...");

                audioHandler?.StopRecording();

                if (assistant != null)
                {
                    await assistant.DisposeAsync();
                    assistant = null;
                }

                await Task.Delay(1000);

                // Recreate client (with the ServiceVersion matching the current mode)
                voiceLiveClient = CreateVoiceLiveClient(currentMode);

                // Start new session
                Console.WriteLine($"Reconnecting in {currentMode} mode...");
                var (model, sessionOptions) = CreateSessionOptions(currentMode);

                assistant = new VoiceLiveAssistant(
                    voiceLiveClient,
                    audioHandler!,
                    avatarHandler,
                    currentMode,
                    logger!);

                await assistant.StartAsync(
                    BuildSessionTarget(currentMode, model),
                    sessionOptions);

                Console.WriteLine("Reconnection successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reconnection failed: {ex.Message}");
                logger?.LogError(ex, "Error during reconnection");
            }
        }

        private static async Task CleanupAsync()
        {
            Console.WriteLine("Cleaning up...");

            audioHandler?.StopRecording();

            if (assistant != null)
            {
                await assistant.DisposeAsync();
                assistant = null;
            }

            avatarHandler?.Dispose();
            avatarHandler = null;

            audioHandler?.Dispose();
            audioHandler = null;

            voiceLiveClient = null;

            telemetryListener?.Dispose();
            telemetryListener = null;

            Console.WriteLine("Goodbye!");
        }

        #endregion
    }
}
