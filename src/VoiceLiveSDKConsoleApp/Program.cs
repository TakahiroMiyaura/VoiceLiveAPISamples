// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text;
using Azure;
using Azure.AI.VoiceLive;
using Azure.Identity;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
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
        private static string azureEndpoint = "<your Azure AI Services Endpoint>";
        private static string agentProjectName = "<your Azure AI Foundry Project Name>";
        private static string agentId = "<your Azure AI Agent Id>";
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
                    model,
                    sessionOptions,
                    currentMode != ConnectionMode.AIModel ? agentProjectName : null,
                    currentMode != ConnectionMode.AIModel ? agentId : null);

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
                        case ConsoleKey.C:
                            await ClearAudioAsync();
                            break;
                        case ConsoleKey.S:
                            ShowStatus();
                            break;
                        case ConsoleKey.V:
                            if (avatarHandler != null)
                                avatarHandler.ToggleVideoStreaming();
                            else
                                Console.WriteLine("Video streaming is only available in Avatar mode");
                            break;
                        case ConsoleKey.F:
                            if (avatarHandler != null)
                                avatarHandler.ShowStreamingInfo();
                            else
                                Console.WriteLine("Avatar streaming is only available in Avatar mode");
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
            Console.WriteLine("- Press 'R' to start/stop recording");
            Console.WriteLine("- Press 'P' to start/stop playback");
            Console.WriteLine("- Press 'M' to switch mode (requires reconnection)");
            Console.WriteLine("- Press 'C' to clear audio queue");
            Console.WriteLine("- Press 'S' to show detailed status");
            Console.WriteLine("- Press 'V' to toggle avatar video streaming (Avatar mode only)");
            Console.WriteLine("- Press 'F' to show avatar streaming information (Avatar mode only)");
            Console.WriteLine("- Press 'T' to test connection and reconnect if needed");
            Console.WriteLine("- Press 'Q' to quit");
        }

        private static (string model, VoiceLiveSessionOptions options) CreateSessionOptions(ConnectionMode mode)
        {
            string model = "phi4-mm-realtime";

            var options = new VoiceLiveSessionOptions
            {
                Instructions = "You are a helpful AI assistant. Please respond in the same language as the user speaks.",
                InputAudioFormat = InputAudioFormat.Pcm16,
                OutputAudioFormat = OutputAudioFormat.Pcm16,
                Voice = new AzureStandardVoice("ja-JP-Nanami:DragonHDLatestNeural"),
                TurnDetection = new ServerVadTurnDetection
                {
                    Threshold = 0.5f,
                    SilenceDuration = TimeSpan.FromMilliseconds(500),
                    PrefixPadding = TimeSpan.FromMilliseconds(300)
                },
                InputAudioEchoCancellation = new AudioEchoCancellation()
            };

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
                        return ConnectionMode.Avatar;
                    default:
                        Console.Write("Invalid choice. Please enter 1, 2, or 3: ");
                        break;
                }
            }
        }

        private static void InitializeClient()
        {
            Console.WriteLine("Choose authentication method:");
            Console.WriteLine("1. API Key");
            Console.WriteLine("2. Entra ID (DefaultAzureCredential)");
            Console.Write("Enter your choice (1 or 2): ");

            useApiKeyAuth = ChooseAuthMethod() == 1;

            Uri endpoint = new Uri(azureEndpoint);

            if (useApiKeyAuth)
            {
                logger?.LogInformation("Initializing VoiceLiveClient (SDK) with API Key authentication...");
                voiceLiveClient = new VoiceLiveClient(endpoint, new AzureKeyCredential(apiKey));
            }
            else
            {
                logger?.LogInformation("Initializing VoiceLiveClient (SDK) with Entra ID authentication...");
                voiceLiveClient = new VoiceLiveClient(endpoint, new DefaultAzureCredential());
            }

            logger?.LogInformation("VoiceLiveClient (SDK) initialized successfully");
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
                    model,
                    sessionOptions,
                    currentMode != ConnectionMode.AIModel ? agentProjectName : null,
                    currentMode != ConnectionMode.AIModel ? agentId : null);

                Console.WriteLine("Mode switched successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error switching mode: {ex.Message}");
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

                // Recreate client
                Uri endpoint = new Uri(azureEndpoint);
                if (useApiKeyAuth)
                {
                    voiceLiveClient = new VoiceLiveClient(endpoint, new AzureKeyCredential(apiKey));
                }
                else
                {
                    voiceLiveClient = new VoiceLiveClient(endpoint, new DefaultAzureCredential());
                }

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
                    model,
                    sessionOptions,
                    currentMode != ConnectionMode.AIModel ? agentProjectName : null,
                    currentMode != ConnectionMode.AIModel ? agentId : null);

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

            Console.WriteLine("Goodbye!");
        }

        #endregion
    }
}
