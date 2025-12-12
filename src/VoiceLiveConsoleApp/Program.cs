// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.DirectoryServices;
using System.Text;
using System.Text.Json;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server;
using Concentus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI;

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

        var loggerFactory = LoggerFactory.Create(configure =>
        {
            configure.SetMinimumLevel(LogLevel.Error); // Production logging level
            configure.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "[yyyy/MM/dd HH:mm:ss] ";
            });
        });

        LoggerFactoryManager.Set(loggerFactory);
        logger = LoggerFactoryManager.CreateLogger<Program>();


        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        azureIdentityTokenRequestUrl = config["Identity:AzureEndpoint"] ?? azureIdentityTokenRequestUrl;
        azureEndpoint = config["VoiceLiveAPI:AzureEndpoint"] ?? azureEndpoint;
        apiKey = config["AzureAIFoundry:ApiKey"] ?? apiKey;
        agentProjectName = config["AzureAIFoundry:AgentProjectName"] ?? agentProjectName;
        agentId = config["AzureAIFoundry:AgentId"] ?? agentId;
        agentAccessToken = config["AzureAIFoundry:AgentAccessToken"] ?? agentAccessToken;

        Console.WriteLine("Azure VoiceInfo Live API Console Application");
        Console.WriteLine("Enhanced with AIModelClient, AIAgentClient & Avatar Video");
        Console.WriteLine("=========================================================");

        try
        {
            // Choose connection mode
            currentMode = ChooseConnectionMode();

            // Initialize client based on mode
            await InitializeClientAsync(currentMode);
            SetupClientEvents();

            // Initialize audio components
            InitializeAudio();

            // Connect to VoiceInfo Live API
            Console.WriteLine($"Connecting to Azure VoiceInfo Live API in {currentMode} mode...");
            var session = ClientSessionUpdate.Default;

            // Configure avatar session based on mode
            if (currentMode == ConnectionMode.Avatar)
            {
                Console.WriteLine("\ud83d\udd0a Avatar mode: Audio output enabled in session");
                logger?.LogInformation("Avatar mode: Audio output enabled in session");
            }
            else
            {
                session.Session.Avatar = null;
                Console.WriteLine("\ud83d\udd0a Non-Avatar mode: Standard audio configuration");
                logger?.LogInformation("Non-Avatar mode: Standard audio configuration");
            }

            await client.ConnectAsync(session);

            // Start audio input
            StartRecording();

            Console.WriteLine("\nReady for conversation!");
            Console.WriteLine("Commands:");
            Console.WriteLine("- Press 'R' to start/stop recording");
            Console.WriteLine("- Press 'P' to start/stop playback");
            Console.WriteLine("- Press 'M' to switch mode and authentication (requires reconnection)");
            Console.WriteLine("- Press 'C' to clear audio queue");
            Console.WriteLine("- Press 'S' to show detailed status");
            Console.WriteLine("- Press 'V' to toggle avatar video streaming (Avatar mode only)");
            Console.WriteLine("- Press 'F' to show avatar streaming information (Avatar mode only)");
            Console.WriteLine("- Press 'T' to test connection and reconnect if needed");
            Console.WriteLine("- Press 'Q' to quit");

            // Main loop
            var running = true;
            while (running)
            {
                var key = Console.ReadKey(true);
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
    ///     The VoiceLive API client instance.
    /// </summary>
    private static VoiceLiveAPIClientBase client = null!;

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
    ///     Current authentication type for reconnection purposes.
    /// </summary>
    private static AuthenticationType currentAuthType;

    /// <summary>
    ///     Current access token for reconnection purposes.
    /// </summary>
    private static string currentAccessToken = string.Empty;

    private static readonly Queue<byte[]> AudioQueue = new();

    private static Task audioPlaybackTask = Task.CompletedTask;

    #endregion

    #region Private Methods

    /// <summary>
    ///     Prompts the user to choose a connection mode.
    /// </summary>
    /// <returns>The selected connection mode.</returns>
    private static ConnectionMode ChooseConnectionMode()
    {
        Console.WriteLine("Choose connection mode:");
        Console.WriteLine("1. AI Model Mode");
        Console.WriteLine("2. AI Agent Mode");
        Console.WriteLine("3. Avatar Mode (with video streaming)");
        Console.Write("Enter your choice (1, 2, or 3): ");

        while (true)
        {
            try
            {
                var input = Console.ReadLine();
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
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error reading console input");
                throw;
            }
        }
    }

    /// <summary>
    ///     Initializes the VoiceLive API client based on the specified mode.
    /// </summary>
    /// <param name="mode">The connection mode to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task InitializeClientAsync(ConnectionMode mode)
    {
        Console.WriteLine("Choose authentication method:");
        Console.WriteLine("1. API Key");
        Console.WriteLine("2. Entra ID (DefaultAzureCredential)");
        Console.Write("Enter your choice (1 or 2): ");

        var authChoice = ChooseAuthMethod();
        var authMethod = authChoice == 1
            ? AuthenticationHelper.AuthenticationMethod.ApiKey
            : AuthenticationHelper.AuthenticationMethod.EntraId;

        string accessToken;
        AuthenticationType authenticationType;

        try
        {
            accessToken = await AuthenticationHelper.GetAccessTokenAsync(
                authMethod,
                apiKey,
                azureIdentityTokenRequestUrl
            );

            // Set authentication type based on method
            authenticationType = authMethod == AuthenticationHelper.AuthenticationMethod.ApiKey
                ? AuthenticationType.ApiKey
                : AuthenticationType.BearerToken;

            // Store for reconnection
            currentAuthType = authenticationType;
            currentAccessToken = accessToken;

            logger?.LogInformation(" Using {authMethod} authentication (Type: {authenticationType})", authMethod,
                authenticationType);
        }
        catch (Exception ex)
        {
            logger?.LogError(" Authentication failed: {ex.Message}", ex.Message);
            throw;
        }

        switch (mode)
        {
            case ConnectionMode.AIModel:
                logger?.LogInformation("Initializing AI Model client...");
                client = new AIModelClient(azureEndpoint, accessToken, authenticationType);
                break;

            case ConnectionMode.AIAgent:
                logger?.LogInformation("Initializing AI Agent client...");
                client = new AIAgentClient(azureEndpoint, accessToken, authenticationType, agentProjectName, agentId);
                break;

            case ConnectionMode.Avatar:
                logger?.LogInformation("Initializing Avatar AI Agent client...");
                client = new AIAgentClient(azureEndpoint, accessToken, authenticationType, agentProjectName, agentId);
                break;

            default:
                throw new ArgumentException($"Unsupported mode: {mode}");
        }
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
                var input = Console.ReadLine();
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

            // Disconnect current client
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
                // AvatarClient doesn't have explicit cleanup methods, but setting to null will allow GC
                avatarClient = null;
            }

            await client.DisconnectAsync();
            client.Dispose();

            // Choose new mode
            var newMode = ChooseConnectionMode();

            // Initialize new client
            await InitializeClientAsync(newMode);
            SetupClientEvents();

            // Reconnect
            Console.WriteLine($"Reconnecting in {newMode} mode...");
            await client.ConnectAsync(ClientSessionUpdate.Default);
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
        waveProvider = new BufferedWaveProvider(new WaveFormat(SampleRate, BitsPerSample, Channels))
        {
            BufferLength = SampleRate * Channels * 2 * 10, // 10 seconds buffer
            DiscardOnBufferOverflow = true
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
                logger?.LogInformation("Opus decoder initialized for Avatar mode: {sampleRate}Hz, {channels} channels",
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

            logger?.LogInformation("Audio initialized for regular mode: {sampleRate}Hz, {channels} channel", SampleRate,
                Channels);
        }
    }

    /// <summary>
    ///     Sets up event handlers for the VoiceLive API client.
    /// </summary>
    private static void SetupClientEvents()
    {
        serverManager = new ServerMessageHandlerManager();
        avatarManager = new AvatarMessageHandlerManager();
        client.AddMessageHandlerManager(serverManager);

        // Add avatar manager only if in Avatar mode, prioritize audio handler
        if (currentMode == ConnectionMode.Avatar)
        {
            client.AddMessageHandlerManager(avatarManager);
            logger?.LogInformation("Avatar mode: Added avatar message handler");
        }
        else
        {
            logger?.LogInformation("Non-Avatar mode: Using standard message handlers only");
        }

        // Initialize avatar client for Avatar mode
        if (currentMode == ConnectionMode.Avatar)
        {
            avatarClient = new AvatarClient();
            logger?.LogInformation("Avatar client initialized for WebRTC streaming");
        }

        serverManager.OnAudioDeltaReceived += response =>
        {
            var pcmData = Convert.FromBase64String(response.Delta);

            // Enhanced logging for Avatar mode audio debugging
            logger?.LogInformation(
                "Audio data received via ServerManager - Index:{index}, Mode: {mode}, Size: {size} bytes, Playing: {playing}",
                response.ContentIndex, currentMode, pcmData.Length, isPlaying);

            // In Avatar mode, skip ServerManager audio processing as it will be handled by AvatarVideoStreamer
            if (currentMode == ConnectionMode.Avatar)
            {
                logger?.LogInformation("Avatar mode: Skipping ServerManager audio processing");
                return;
            }

            if (pcmData.Length > 0)
            {
                logger?.Log(LogLevel.Debug, "type : {type}, data {size} bytes.", response.Type, pcmData.Length);
                lock (AudioQueue)
                {
                    AudioQueue.Enqueue(pcmData);

                    audioPlaybackTask = Task.Run(async () =>
                    {
                        var minBuffer = BitsPerSample / 8 * Channels * SampleRate * 3;
                        while (true)
                        {
                            await Task.Delay(300);

                            while (waveProvider.BufferedBytes < waveProvider.BufferLength - minBuffer &&
                                   AudioQueue.Count > 0)
                            {
                                byte[] data;
                                lock (AudioQueue)
                                    data = AudioQueue.Dequeue();

                                lock (waveProvider)
                                {
                                    waveProvider.AddSamples(data, 0, data.Length);
                                }
                            }
                        }
                    });
                }

                // Start audio playback for non-Avatar modes
                if (!isPlaying)
                {
                    logger?.LogInformation("Starting audio playback for {mode} mode", currentMode);
                    StartAudioPlayback();
                }
            }
            else
            {
                logger?.LogWarning("Empty audio data received in {mode} mode", currentMode);
            }
        };

        serverManager.OnTranscriptionReceived += transcription =>
        {
            logger?.Log(LogLevel.Trace, "[message]: {Transcript}", transcription.Transcript);
        };

        serverManager.OnSessionUpdateReceived += async sessionUpdate =>
        {
            logger?.Log(LogLevel.Trace, "type : {Type}", sessionUpdate.Type);

            if (sessionUpdate.Session.Avatar != null)
            {
                if (avatarClient == null)
                {
                    logger?.LogError(
                        "Avatar session received but avatarClient is null - this should not happen in Avatar mode");
                    return;
                }

                var ics = sessionUpdate.Session.Avatar.IceServers[0];
                logger?.LogInformation("Starting WebRTC connection with ICE servers: {urls}",
                    string.Join(", ", ics.Urls));
                await avatarClient.AvatarConnectAsync(ics, client);
                logger?.LogInformation("WebRTC connection initiated successfully");

                // Initialize avatar video streaming (RTP-based V6)
                if (currentMode == ConnectionMode.Avatar && avatarVideoStreamer == null)
                {
                    try
                    {
                        avatarVideoStreamer = new AvatarVideoStreamer(avatarClient,
                            logger ?? throw new NullReferenceException("logger is null.jjj"));

                        if (!avatarVideoStreamer.StartStreaming())
                        {
                            logger?.LogError("Failed to start avatar video streaming - check FFmpeg availability");
                            avatarVideoStreamer?.Dispose();
                            avatarVideoStreamer = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Exception during avatar video streaming initialization");
                        avatarVideoStreamer?.Dispose();
                        avatarVideoStreamer = null;
                    }
                }

                // Ensure audio is ready for Avatar mode
                logger?.LogInformation("Avatar connected, audio playback prepared");
            }

            StartRecording();
        };

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
            logger?.LogTrace("Remote SDP set successfully, WebRTC connection should be established");
        };

        serverManager.OnErrorReceived += response =>
        {
            logger?.LogError("Error received: {Type} - {Response}", response.Type, JsonSerializer.Serialize(response));
            if (response.ErrorDetail != null)
            {
                logger?.LogError("   Code: {Code}", response.ErrorDetail.Code);
                logger?.LogError("   Message: {Message}", response.ErrorDetail.Message);
                if (!string.IsNullOrEmpty(response.ErrorDetail.Type))
                {
                    logger?.LogError("   Type: {ErrorType}", response.ErrorDetail.Type);
                }
            }
        };

        serverManager.OnResponseTextDoneReceived += response =>
        {
            logger?.LogTrace("{Type} : {Text}", response.Type, response.Text);
        };


        serverManager.OnConversationCreatedReceived += DebugMessages;
        serverManager.OnConversationItemCreatedReceived += response =>
        {
            var transcripts = "";
            if (response.Item?.Content != null && response.Item.Content.Length > 0)
            {
                transcripts = response.Item?.Content?.Select(x => x.Transcript).Aggregate((a, b) => a + "\n" + b) ?? "";
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

        // Auto-stop recording when speech is detected as stopped by Azure AI
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
        serverManager.OnResponseFunctionCallArgumentsDeltaReceived += DebugMessages;
        serverManager.OnResponseFunctionCallArgumentsDoneReceived += DebugMessages;
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

    private static void DebugMessages(MessageBase response)
    {
        logger?.LogTrace("received: {Type}", response.Type);
    }

    private static async void OnAudioDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (client != null && isRecording && e.BytesRecorded > 0)
        {
            try
            {
                var audioData = new byte[e.BytesRecorded];
                Array.Copy(e.Buffer, 0, audioData, 0, e.BytesRecorded);

                await new InputAudioBufferAppend().SendAsync(audioData, client);
            }
            catch (Exception ex)
            {
                logger?.LogError("Error sending audio data: {Message}", ex.Message);
            }
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
        if (!isPlaying)
        {
            try
            {
                Console.WriteLine("Starting playback...");
                waveOut.Play();
                isPlaying = true;
                Console.WriteLine("Playback started");
            }
            catch (Exception ex)
            {
                logger?.LogError("Error starting playback: {Message}", ex.Message);
            }
        }
        else
        {
            Console.WriteLine("Already playing");
        }
    }

    private static void StopPlayback()
    {
        if (isPlaying)
        {
            try
            {
                waveOut.Stop();
                isPlaying = false;
                Console.WriteLine("Playback stopped");
            }
            catch (Exception ex)
            {
                logger?.LogError("Error ⚠️ Audio frame playback: {Message}", ex.Message);
            }
        }
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
    ///     Clears the audio queue in the VoiceLive API client.
    /// </summary>
    private static void ClearAudioQueue()
    {
        if (client != null)
        {
            Console.WriteLine(" Clearing audio queue...");
            client.ClearAudioQueue();
            Console.WriteLine(" Audio queue cleared");
        }
        else
        {
            Console.WriteLine(" Client not initialized");
        }
    }

    /// <summary>
    ///     Shows the current status of the application and audio components.
    /// </summary>
    private static void ShowStatus()
    {
        Console.WriteLine("\n=== Current Status ===");
        Console.WriteLine($"Recording: {(isRecording ? " ON" : " OFF")}");
        Console.WriteLine($"Playback: {(isPlaying ? " ON" : " OFF")}");

        if (client != null)
        {
            // Determine client type and mode
            if (client is AIModelClient modelClient)
            {
                Console.WriteLine("Connection Mode: AI Model");
                Console.WriteLine($"Model: {modelClient.Model}");
            }
            else if (client is AIAgentClient agentClient)
            {
                Console.WriteLine("Connection Mode: AI Agent");
                Console.WriteLine($"Project: {agentClient.ProjectName}");
                Console.WriteLine($"Agent ID: {agentClient.AgentId}");
            }

            Console.WriteLine("Auth Method: Token-based");
            Console.WriteLine($"Endpoint: {client.Endpoint}");

            var queueCount = client.GetAudioQueueCount();
            Console.WriteLine($"Audio Queue: {queueCount} chunks");
        }

        if (waveProvider != null)
        {
            var bufferedDuration = waveProvider.BufferedDuration;
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

        if (client != null)
        {
            await client.DisconnectAsync();
            client.Dispose();
        }


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
        if (!isPlaying && waveOut != null)
        {
            try
            {
                Console.WriteLine("🔊 Starting audio playback...");
                waveOut.Play();
                isPlaying = true;
                Console.WriteLine("✅ Audio playback started");
                logger?.LogInformation("Audio playback started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error starting audio playback: {ex.Message}");
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

            if (client == null)
            {
                Console.WriteLine("❌ No client initialized");
                return;
            }

            // Check connection state
            var isConnected = await TestConnection();

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

            // Dispose current client
            if (client != null)
            {
                try
                {
                    await client.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Error disconnecting client during reconnection");
                }

                client.Dispose();
                client = null!;
            }

            // Wait a moment before reconnecting
            await Task.Delay(1000);

            // Recreate client with stored credentials
            await RecreateClient();
            SetupClientEvents();

            // Reconnect
            Console.WriteLine($"🔄 Reconnecting in {currentMode} mode...");
            var session = CreateClientSessionUpdate(currentMode);

            await client?.ConnectAsync(session)!;

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
    /// Create ClientSessionUpdate message adjusted by connection mode in Azure AI Foundry.
    /// </summary>
    /// <param name="mode">ConnectionMode</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static ClientSessionUpdate CreateClientSessionUpdate(ConnectionMode mode)
    {
        ClientSessionUpdate session = ClientSessionUpdate.Default;
        switch (mode)
        {
            case ConnectionMode.AIAgent:
            case ConnectionMode.AIModel:
                session.Session.Avatar = null;
                break;
            case ConnectionMode.Avatar:
                session.Session.TurnDetection = new TurnDetection
                {
                    Type = "server_vad",
                    EndOfUtteranceDetection = new
                    {
                        model = "semantic_detection_v1",
                        threshold = 0.1,
                        timeout = 4
                    }
                };
                session.Session.Avatar = new Avatar
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
            default:
                throw new InvalidOperationException();
        }

        return session;
    }

    /// <summary>
    ///     Tests if the current connection is healthy.
    /// </summary>
    /// <returns>True if connection is healthy, false otherwise.</returns>
    private static async Task<bool> TestConnection()
    {
        try
        {
            if (client == null)
                return false;

            // Try to clear audio queue as a simple connection test
            client.ClearAudioQueue();

            // Wait a short time to see if any immediate errors occur
            await Task.Delay(100);

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Connection test failed");
            return false;
        }
    }

    /// <summary>
    ///     Recreates the client with stored credentials.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task RecreateClient()
    {
        try
        {
            switch (currentMode)
            {
                case ConnectionMode.AIModel:
                    logger?.LogInformation("Recreating AI Model client...");
                    client = new AIModelClient(azureEndpoint, currentAccessToken, currentAuthType);
                    break;

                case ConnectionMode.AIAgent:
                    logger?.LogInformation("Recreating AI Agent client...");
                    client = new AIAgentClient(azureEndpoint, currentAccessToken, currentAuthType, agentProjectName,
                        agentId);
                    break;

                case ConnectionMode.Avatar:
                    logger?.LogInformation("Recreating Avatar AI Agent client...");
                    client = new AIAgentClient(azureEndpoint, currentAccessToken, currentAuthType, agentProjectName,
                        agentId);
                    break;

                default:
                    throw new ArgumentException($"Unsupported mode: {currentMode}");
            }

            return Task.CompletedTask;
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