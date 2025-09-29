// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server;
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
    AIAgent
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
    private static async Task Main()
    {
        var loggerFactory = LoggerFactory.Create(configure =>
        {
            configure.SetMinimumLevel(LogLevel.Trace);

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
        Console.WriteLine("Enhanced with AIModelClient & AIAgentClient");
        Console.WriteLine("==========================================");

        try
        {
            // Choose connection mode
            var mode = ChooseConnectionMode();

            // Initialize client based on mode
            await InitializeClientAsync(mode);
            SetupClientEvents();

            // Initialize audio components
            InitializeAudio();

            // Connect to VoiceInfo Live API
            Console.WriteLine($"Connecting to Azure VoiceInfo Live API in {mode} mode...");
            var session = ClientSessionUpdate.Default;
            session.Session.Avatar = null;
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
                    case ConsoleKey.Q:
                        running = false;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
        finally
        {
            await Cleanup();
        }
    }

    #endregion

    #region Static Fields and Constants

    /// <summary>
    ///     Audio sample rate in Hz.
    /// </summary>
    private const int SampleRate = 24000;

    /// <summary>
    ///     Number of audio channels.
    /// </summary>
    private const int Channels = 1;

    /// <summary>
    ///     Bits per audio sample.
    /// </summary>
    private const int BitsPerSample = 16;

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
        Console.Write("Enter your choice (1 or 2): ");

        while (true)
        {
            var key = Console.ReadKey(true);
            Console.WriteLine();

            switch (key.KeyChar)
            {
                case '1':
                    Console.WriteLine("Selected: AI Model Mode");
                    return ConnectionMode.AIModel;
                case '2':
                    Console.WriteLine("Selected: AI Agent Mode");
                    return ConnectionMode.AIAgent;
                default:
                    Console.Write("Invalid choice. Please enter 1 or 2: ");
                    break;
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
            var key = Console.ReadKey(true);
            Console.WriteLine();

            switch (key.KeyChar)
            {
                case '1':
                    return 1;
                case '2':
                    return 2;
                default:
                    Console.Write("Invalid choice. Please enter 1 or 2: ");
                    break;
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
        // Setup audio input (microphone)
        waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(SampleRate, BitsPerSample, Channels),
            BufferMilliseconds = 100
        };
        waveIn.DataAvailable += OnAudioDataAvailable!;
        waveIn.RecordingStopped += OnRecordingStopped!;

        // Setup audio output (speakers)
        waveOut = new WaveOutEvent();
        waveProvider = new BufferedWaveProvider(new WaveFormat(SampleRate, BitsPerSample, Channels))
        {
            BufferLength = SampleRate * Channels * 2 * 10, // 10 seconds buffer
            DiscardOnBufferOverflow = true
        };
        waveOut.Init(waveProvider);
    }

    /// <summary>
    ///     Sets up event handlers for the VoiceLive API client.
    /// </summary>
    private static void SetupClientEvents()
    {
        var serverManager = new ServerMessageHandlerManager();
        var avatarManager = new AvatarMessageHandlerManager();
        client.AddMessageHandlerManager(serverManager);
        client.AddMessageHandlerManager(avatarManager);

        var avatarClient = new AvatarClient();
        serverManager.OnAudioDeltaReceived += response =>
        {
            var pcmData = Convert.FromBase64String(response.Delta);
            if (pcmData.Length > 0)
            {
                logger?.Log(LogLevel.Debug, " type : {response.type},data {pcmData.Length} bytes.", response.Type,
                    pcmData.Length);
                lock (waveProvider)
                {
                    waveProvider.AddSamples(pcmData, 0, pcmData.Length);
                }
            }
        };

        serverManager.OnTranscriptionReceived += transcription =>
        {
            logger?.Log(LogLevel.Debug, " [message]: {transcription.transcript}", transcription.Transcript);
        };

        serverManager.OnSessionUpdateReceived += async sessionUpdate =>
        {
            logger?.Log(LogLevel.Debug, " type : {sessionUpdate.type}", sessionUpdate.Type);

            if (sessionUpdate.Session.Avatar != null)
            {
                var ics = sessionUpdate.Session.Avatar.IceServers[0];
                await avatarClient.AvatarConnectAsync(ics, client);
                
            }

            StartRecording();
        };

        avatarManager.OnSessionAvatarConnecting += connecting =>
        {
            logger?.Log(LogLevel.Debug, " type : {connecting.type}", connecting.Type);
            avatarClient.AvatarConnecting(connecting.ServerSdp);
        };


        serverManager.OnConversationCreatedReceived += DebugMessages;
        serverManager.OnConversationItemCreatedReceived += response =>
        {
            var transcripts = "";
            if (response.Item?.Content != null && response.Item.Content.Length > 0)
            {
                transcripts = response.Item?.Content?.Select(x => x.Transcript).Aggregate((a, b) => a + "\n" + b) ?? "";
            }

            logger?.Log(LogLevel.Debug,
                " {response.Item?.role}: {transcripts}", response.Item?.Role, transcripts);
        };
        serverManager.OnConversationItemRetrievedReceived += DebugMessages;
        serverManager.OnConversationItemDeletedReceived += DebugMessages;
        serverManager.OnConversationItemInputAudioTranscriptionFailedReceived += DebugMessages;
        serverManager.OnConversationItemTruncatedReceived += DebugMessages;
        serverManager.OnErrorReceived += response =>
        {
            logger?.Log(LogLevel.Debug, " received: {response.type} {JsonSerializer.Serialize(response)}",
                response.Type, JsonSerializer.Serialize(response));
        };
        serverManager.OnInputAudioBufferClearedReceived += DebugMessages;
        serverManager.OnInputAudioBufferCommittedReceived += DebugMessages;
        serverManager.OnInputAudioBufferSpeechStartedReceived += DebugMessages;
        serverManager.OnInputAudioBufferSpeechStoppedReceived += DebugMessages;
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
                $" {response.Item.Role}: {response.Item.Content?.Select(x => x.Transcript).Aggregate((a, b) => a + "\n" + b)}");
        };
        serverManager.OnResponseTextDeltaReceived += DebugMessages;
        serverManager.OnResponseTextDoneReceived += response =>
        {
            logger?.Log(LogLevel.Debug, " {response.type} : {response.text}", response.Type, response.Text);
        };
        serverManager.OnSessionCreatedReceived += DebugMessages;
    }

    private static void DebugMessages(MessageBase response)
    {
        logger?.Log(LogLevel.Debug, " received: {response.type}", response.Type);
    }

    /// <summary>
    ///     Handles audio data available events from the microphone.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The wave input event arguments.</param>
    private static async void OnAudioDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (client != null && isRecording && e.BytesRecorded > 0)
        {
            try
            {
                // Convert from the input format to the required format if necessary
                var audioData = new byte[e.BytesRecorded];
                Array.Copy(e.Buffer, 0, audioData, 0, e.BytesRecorded);

                // Send audio data to the API (fire and forget)
                await new InputAudioBufferAppend().SendAsync(audioData, client);
            }
            catch (Exception ex)
            {
                logger?.LogError(" Error sending audio data: {ex.Message}", ex.Message);
            }
        }
    }

    /// <summary>
    ///     Handles recording stopped events.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The stopped event arguments.</param>
    private static void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        Console.WriteLine(" Recording stopped");
        if (e.Exception != null)
        {
            logger?.LogError("Recording error: {e.Exception.Message}", e.Exception.Message);
        }
    }

    /// <summary>
    ///     Starts audio recording from the microphone.
    /// </summary>
    private static void StartRecording()
    {
        if (!isRecording)
        {
            try
            {
                Console.WriteLine(" Starting microphone...");
                waveIn.StartRecording();
                isRecording = true;
                Console.WriteLine(" Recording started");
            }
            catch (Exception ex)
            {
                logger?.LogError(" Error starting recording: {ex.Message}", ex.Message);
            }
        }
        else
        {
            Console.WriteLine(" Already recording");
        }
    }

    /// <summary>
    ///     Stops audio recording from the microphone.
    /// </summary>
    private static void StopRecording()
    {
        if (isRecording)
        {
            try
            {
                waveIn.StopRecording();
                isRecording = false;
                Console.WriteLine(" Recording stopped");
            }
            catch (Exception ex)
            {
                logger?.LogError(" Error stopping recording: {ex.Message}", ex.Message);
            }
        }
    }

    /// <summary>
    ///     Starts audio playback to the speakers.
    /// </summary>
    private static void StartPlayback()
    {
        if (!isPlaying)
        {
            try
            {
                Console.WriteLine(" Starting playback...");
                waveOut.Play();
                isPlaying = true;
                Console.WriteLine(" Playback started");
            }
            catch (Exception ex)
            {
                logger?.LogError(" Error starting playback: {ex.Message}", ex.Message);
            }
        }
        else
        {
            Console.WriteLine(" Already playing");
        }
    }

    /// <summary>
    ///     Stops audio playback to the speakers.
    /// </summary>
    private static void StopPlayback()
    {
        if (isPlaying)
        {
            try
            {
                waveOut.Stop();
                isPlaying = false;
                Console.WriteLine(" Playback stopped");
            }
            catch (Exception ex)
            {
                logger?.LogError(" Error stopping playback: {ex.Message}", ex.Message);
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

        if (client != null)
        {
            await client.DisconnectAsync();
            client.Dispose();
        }

        Console.WriteLine("Goodbye!");
    }

    #endregion
}