// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.InputAudioBuffers;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Sessions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.IO;
using System.Text;
using System.Text.Json;
using static Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.AuthenticationType;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI;

public class Test : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}

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

internal class Program
{

    private const int SampleRate = 24000;
    private const int Channels = 1;
    private const int BitsPerSample = 16;
    private static VoiceLiveAPIClientBase client = null!;
    private static WaveInEvent waveIn = null!;
    private static WaveOutEvent waveOut = null!;
    private static BufferedWaveProvider waveProvider = null!;
    private static bool isRecording;
    private static bool isPlaying;
    private static ILogger? logger = new Test();

    private static string azureEndpoint = "<your Azure AI Services Endpoint>";
    private static string agentProjectName = "<your Azure AI Foundry Project Name>";
    private static string agentId = "<your Azure AI Agent Id>";
    private static string azureIdentityTokenRequestUrl = "<Token request url(ex:https://ai.azure.com/.default)>";

    private static string apiKey = "<Azure AI Foundry API Key>";

    private static string agentAccessToken = "<Azure AI Foundry API Key>";

    private static async Task Main(string[] args)
    {
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
            await client.ConnectAsync(ClientSessionUpdate.Default);

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

    private static async Task InitializeClientAsync(ConnectionMode mode)
    {
        Console.WriteLine("Choose authentication method:");
        Console.WriteLine("1. API Key");
        Console.WriteLine("2. Entra ID (DefaultAzureCredential)");
        Console.Write("Enter your choice (1 or 2): ");

        var authChoice = ChooseAuthMethod();
        var authMethod = authChoice == 1 ? AuthenticationHelper.AuthenticationMethod.ApiKey : AuthenticationHelper.AuthenticationMethod.EntraId;
        
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
            authenticationType = authMethod == AuthenticationHelper.AuthenticationMethod.ApiKey ? ApiKey : BearerToken;
            
            Console.WriteLine($" Using {authMethod} authentication (Type: {authenticationType})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Authentication failed: {ex.Message}");
            throw;
        }

        switch (mode)
        {
            case ConnectionMode.AIModel:
                Console.WriteLine("Initializing AI Model client...");
                client = new AIModelClient(azureEndpoint, accessToken, authenticationType);
                break;

            case ConnectionMode.AIAgent:
                Console.WriteLine("Initializing AI Agent client...");
                client = new AIAgentClient(azureEndpoint, accessToken, authenticationType, agentProjectName, agentId);
                break;

            default:
                throw new ArgumentException($"Unsupported mode: {mode}");
        }
        
            client.Logger = logger;
    }

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

    private static void SetupClientEvents()
    {

        client.OnVideoFrameReceived += (response, ssrc, frame, formart) =>
        {
            logger?.Log(LogLevel.Information, $" type : {formart.Codec}/{formart.ClockRate},frame {frame.Length} bytes.");

        };
        client.OnAudioDeltaReceived += response =>
        {

            var pcmData = Convert.FromBase64String(response.delta);
            if (pcmData.Length > 0)
            {
                logger?.Log(LogLevel.Information, $" type : {response.type},data {pcmData.Length} bytes.");
                waveProvider.AddSamples(pcmData, 0, pcmData.Length);
            }
        };

        client.OnTranscriptionReceived += transcription =>
        {
            Console.WriteLine($" [message]: {transcription.transcript}");
        };

        client.OnSessionUpdateReceived += sessionUpdate =>
        {
            logger?.Log(LogLevel.Information, $" type : {sessionUpdate.type}");

            if (sessionUpdate.session.avatar != null)
            {
                var ics = sessionUpdate.session.avatar.ice_servers[0];

                client.AvatarConnectAsync(ics);
            }

            StartRecording();

        };

        client.OnSessionAvatarConnecting += connecting =>
        {

            logger?.Log(LogLevel.Information, $" type : {connecting.type}");
            client.AvatarConnectingAsync(connecting.server_sdp);
        };


        client.OnConversationCreatedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnConversationItemCreatedReceived += response =>
        {
            Console.WriteLine(
                $" {response.item?.role}: {response.item?.content?.Select(x => x.transcript).Aggregate((a, b) => a + "\n" + b)}");
        };
        client.OnConversationItemRetrievedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnConversationItemDeletedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnConversationItemInputAudioTranscriptionFailedReceived += response =>
        {
            logger?.Log(LogLevel.Information, $" received: {response.type}");
        };
        client.OnConversationItemTruncatedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnErrorReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type} {JsonSerializer.Serialize(response)}"); };
        client.OnInputAudioBufferClearedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnInputAudioBufferCommittedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnInputAudioBufferSpeechStartedReceived += response =>
        {
            logger?.Log(LogLevel.Information, $" received: {response.type}");
        };
        client.OnInputAudioBufferSpeechStoppedReceived += response =>
        {
            logger?.Log(LogLevel.Information, $" received: {response.type}");
        };
        client.OnOutputAudioBufferClearedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnOutputAudioBufferStartedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnOutputAudioBufferStoppedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnRateLimitsUpdatedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnResponseAnimationVisemeDoneReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnResponseAnimationVisemeDeltaReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnResponseAudioDoneReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnResponseAudioTranscriptDeltaReceived += response =>
        {
            logger?.Log(LogLevel.Information, $" received: {response.type}");
        };
        client.OnResponseAudioTranscriptDoneReceived += response =>
        {
            logger?.Log(LogLevel.Information, $" received: {response.type}");
        };
        client.OnResponseContentPartAddedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnResponseContentPartDoneReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnResponseCreatedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnResponseDoneReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnResponseFunctionCallArgumentsDeltaReceived += response =>
        {
            logger?.Log(LogLevel.Information, $" received: {response.type}");
        };
        client.OnResponseFunctionCallArgumentsDoneReceived += response =>
        {
            logger?.Log(LogLevel.Information, $" received: {response.type}");
        };
        client.OnResponseOutputItemAddedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnResponseOutputItemDoneReceived += response =>
        {
            Console.WriteLine(
                $" {response.item.role}: {response.item.content?.Select(x => x.transcript).Aggregate((a, b) => a + "\n" + b)}");
        };
        client.OnResponseTextDeltaReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
        client.OnResponseTextDoneReceived += response => { logger?.Log(LogLevel.Information, $" {response.type} : {response.text}"); };
        client.OnSessionCreatedReceived += response => { logger?.Log(LogLevel.Information, $" received: {response.type}"); };
    }

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
                await new InputAudioBufferAppend().SendAsync(audioData,client);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error sending audio data: {ex.Message}");
            }
        }
    }

    private static void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        Console.WriteLine(" Recording stopped");
        if (e.Exception != null)
        {
            Console.WriteLine($"Recording error: {e.Exception.Message}");
        }
    }

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
                Console.WriteLine($" Error starting recording: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine(" Already recording");
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
                Console.WriteLine(" Recording stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error stopping recording: {ex.Message}");
            }
        }
    }

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
                Console.WriteLine($" Error starting playback: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine(" Already playing");
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
                Console.WriteLine(" Playback stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error stopping playback: {ex.Message}");
            }
        }
    }

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
}