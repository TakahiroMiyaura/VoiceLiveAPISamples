# Azure Voice Live API Console Application

[![License: BSL-1.0](https://img.shields.io/badge/License-BSL--1.0-blue.svg)](https://opensource.org/licenses/BSL-1.0)

A .NET 8 console application and reusable client library for real-time voice conversation with Azure AI Foundry's Voice Live API. Supports **AI Model mode**, **AI Agent mode**, and **Avatar mode** with microphone input, speaker output, and real-time video streaming.

[![Foundry VoiceLiveAPI AvatarDemo](https://img.youtube.com/vi/lZ5fp42zWNs/0.jpg)](https://www.youtube.com/watch?v=lZ5fp42zWNs)

## Features

- **Triple Connection Modes**:
  - **AI Model Mode**: Direct connection to Azure AI models (GPT-4o, etc.)
  - **AI Agent Mode**: Connection to custom AI agents with specialized configurations
  - **Avatar Mode**: WebRTC video streaming with real-time H.264 video and Opus audio

- **Multiple Authentication Methods**:
  - API Key authentication
  - Azure SDK credential (AzureKeyCredential, TokenCredential)
  - Bearer Token (for Unity/non-Azure environments)

- **Modern Async Patterns**:
  - Event-based handlers via `ServerMessageHandlerManager`
  - IAsyncEnumerable stream pattern via `GetUpdatesAsync()`

## Required Packages

| Package Name                                 | Version         | Purpose                    |
|----------------------------------------------|-----------------|----------------------------|
| Azure.Identity                               | 1.14.2          | Azure authentication       |
| Microsoft.Extensions.Configuration           | 9.0.8           | Configuration management   |
| Microsoft.Extensions.Configuration.UserSecrets | 9.0.8        | Secure configuration       |
| Microsoft.Extensions.Logging                | 9.0.8           | Logging infrastructure     |
| System.Text.Json                             | 9.0.8           | JSON serialization         |
| NAudio                                       | 2.2.1           | Cross-platform audio      |
| SIPSorcery                                   | 8.0.23          | WebRTC implementation      |
| SIPSorceryMedia.Abstractions                | 8.0.12          | Media format abstractions |
| Concentus                                    | 2.2.2           | Opus audio codec           |
| FFMpegCore                                   | 5.1.0           | FFmpeg integration         |
| CliWrap                                      | 3.6.6           | Command line process wrapper |

### External Dependencies (Avatar Mode)
- **FFmpeg**: Required for H.264 video processing and MPEGTS container generation
- **FFplay**: Required for video playback and testing

## Quick Start

### Prerequisites

1. **FFmpeg Installation** (required for Avatar mode):
   ```bash
   # Download from https://ffmpeg.org/download.html
   # Ensure ffmpeg and ffplay are in your PATH
   ffmpeg -version
   ffplay -version
   ```

2. **Azure Resources Setup**:
   - Azure AI Foundry(Old version) & Project
   - AI Agent (optional, for AI Agent mode)
   - Note the **Project Endpoint** and **API KEY**

### Installation

1. Clone the sample project.

```powershell
PS C:\hoge> git clone https://github.com/TakahiroMiyaura/VoiceLiveAPISamples.git
PS C:\hoge> cd VoiceLiveAPISamples
```

2. Register the Azure AI Foundry endpoint.

**When using only EntraID login (az login, etc.), 'AzureAIFoundry:ApiKey' is not required.**

```powershell
PS D:\hoge\VoiceLiveAPISamples> dotnet user-secrets init --project src\VoiceLiveConsoleApp
PS D:\hoge\VoiceLiveAPISamples> dotnet user-secrets set "Identity:AzureEndpoint" "<Token request url(ex:https://ai.azure.com/.default)>" --project src\VoiceLiveConsoleApp
PS D:\hoge\VoiceLiveAPISamples> dotnet user-secrets set "AzureAIFoundry:AgentProjectName" "<your Azure AI Foundry Project Name>" --project src\VoiceLiveConsoleApp
PS D:\hoge\VoiceLiveAPISamples> dotnet user-secrets set "VoiceLiveAPI:AzureEndpoint" "<your Azure AI Services Endpoint>" --project src\VoiceLiveConsoleApp
PS D:\hoge\VoiceLiveAPISamples> dotnet user-secrets set "AzureAIFoundry:AgentId" "<your Azure AI Agent Id>" --project src\VoiceLiveConsoleApp
PS D:\hoge\VoiceLiveAPISamples> dotnet user-secrets set "AzureAIFoundry:AgentAccessToken" "<Azure AI Foundry API Key>" --project src\VoiceLiveConsoleApp
PS D:\hoge\VoiceLiveAPISamples> dotnet user-secrets set "AzureAIFoundry:ApiKey" "<Azure AI Foundry API Key>" --project src\VoiceLiveConsoleApp
```

3. Build the console application.
```powershell
PS D:\hoge\VoiceLiveAPISamples > dotnet build src\VoiceLiveConsoleApp
```

4. Run the application.
```powershell
PS D:\hoge\VoiceLiveAPISamples > dotnet run --project src/VoiceLiveConsoleApp
```

## Usage

### Console Application

Run the application and choose between AI Model, AI Agent, or Avatar mode:

> [!CAUTION]
> The API key can only be used in AI Model Mode.
> For AI Agent Mode and Avatar Mode, please use Entra ID authentication (DefaultAzureCredential).
> Before using Entra ID authentication, ensure you have logged in using the Azure CLI (`az login`).

```
Choose connection mode:
1. AI Model Mode
2. AI Agent Mode
3. Avatar Mode (with video streaming)
Enter your choice (1, 2, or 3): 3

Choose authentication method:
1. API Key
2. Entra ID (DefaultAzureCredential)
Enter your choice (1 or 2): 2

Commands:
- Press 'R' to start/stop recording (auto-stops when speech ends)
- Press 'P' to start/stop playback
- Press 'M' to switch modes
- Press 'C' to clear audio queue
- Press 'S' to show detailed status
- Press 'V' to toggle avatar video streaming (Avatar mode only)
- Press 'F' to start FFplay (Avatar mode only)
- Press 'T' to test connection and reconnect if needed
- Press 'Q' to quit

**Recording Auto-Stop**: Recording automatically stops when Azure AI detects your speech has ended. This prevents environmental noise from interrupting AI responses. You can still manually stop by pressing 'R' again.
```

#### Avatar Mode Features
- **Real-time Video**: H.264 video streaming with automatic SPS/PPS reconstruction
- **Audio Integration**: Opus audio capture and MPEGTS multiplexing
- **FFplay Integration**: Press 'F' to launch video playback

## API Reference

### Authentication Methods

The VoiceLive API supports multiple authentication methods:

```csharp
using Azure;
using Azure.Identity;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients;

// 1. API Key authentication (simple string)
var client = new VoiceLiveClient(
    "https://your-resource.cognitiveservices.azure.com",
    "your-api-key");

// 2. AzureKeyCredential authentication
var client = new VoiceLiveClient(
    endpoint,
    new AzureKeyCredential(apiKey));

// 3. TokenCredential authentication (Entra ID)
var client = new VoiceLiveClient(
    endpoint,
    new DefaultAzureCredential());

// 4. TokenCredential with custom scopes
var client = new VoiceLiveClient(
    endpoint,
    new DefaultAzureCredential(),
    new[] { "https://ai.azure.com/.default" });

// 5. Bearer Token (for Unity/non-Azure environments)
var client = new VoiceLiveClient(
    endpoint,
    bearerToken,
    AuthenticationType.BearerToken);
```

### AI Model Mode

```csharp
using Azure;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;

// Create client
var client = new VoiceLiveClient(
    "https://your-resource.cognitiveservices.azure.com",
    new AzureKeyCredential("your-api-key"));

// Start session with model name
var session = await client.StartSessionAsync("gpt-4o");

// Or start with custom options
var options = VoiceLiveSessionOptions.CreateDefault();
options.Model = "gpt-4o";
options.Voice = new Voice
{
    Name = "ja-JP-Nanami:DragonHDLatestNeural",
    Type = "azure-standard"
};
var session = await client.StartSessionAsync(options);

// Setup event handlers
var serverManager = new ServerMessageHandlerManager();
serverManager.OnAudioDeltaReceived += (audioDelta) =>
{
    byte[] pcmData = Convert.FromBase64String(audioDelta.Delta);
    // Handle received audio
};
serverManager.OnTranscriptionReceived += (transcription) =>
{
    Console.WriteLine($"Transcript: {transcription.Transcript}");
};
serverManager.OnErrorReceived += (error) =>
{
    Console.WriteLine($"Error: {error.Type} - {error.Code}");
};
session.AddMessageHandlerManager(serverManager);

// Send audio data
await session.SendInputAudioAsync(audioBytes);

// Cleanup
await session.DisposeAsync();
```

### AI Agent Mode

```csharp
using Azure;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;

// Create client
var client = new VoiceLiveClient(
    "https://your-resource.cognitiveservices.azure.com",
    new AzureKeyCredential("your-api-key"));

// Set agent access token if required
client.AgentAccessToken = "your-agent-access-token";

// Create message handlers before connecting
var serverManager = new ServerMessageHandlerManager();
serverManager.OnAudioDeltaReceived += (audioDelta) =>
{
    byte[] pcmData = Convert.FromBase64String(audioDelta.Delta);
    // Handle received audio
};
serverManager.OnSessionUpdateReceived += (sessionInfo) =>
{
    // Session configuration received from server
    Console.WriteLine($"Session updated: {sessionInfo.Id}");
};

// Start agent session with handlers registered before connecting
var session = await client.StartAgentSessionAsync(
    "your-project-name",
    "your-agent-id",
    VoiceLiveSessionOptions.CreateDefault(),
    new[] { serverManager });

// Send audio data
await session.SendInputAudioAsync(audioBytes);
```

### Avatar Mode

```csharp
using Azure;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars;

// Create client
var client = new VoiceLiveClient(endpoint, new AzureKeyCredential(apiKey));
client.AgentAccessToken = "your-agent-access-token";

// Configure avatar options
var options = VoiceLiveSessionOptions.CreateDefault();
options.Avatar = new Avatar
{
    Character = "lisa",
    Style = "casual-sitting",
    Video = new Video
    {
        BitRate = 2000000,
        Codec = "h264",
        Width = 1920,
        Height = 1080,
        FrameRate = 30
    }
};

// Create handlers
var serverManager = new ServerMessageHandlerManager();
var avatarManager = new AvatarMessageHandlerManager();
var avatarClient = new AvatarClient();

// Handle avatar connection
serverManager.OnSessionUpdateReceived += async (sessionInfo) =>
{
    if (sessionInfo.Avatar?.IceServers != null)
    {
        await avatarClient.AvatarConnectAsync(
            sessionInfo.Avatar.IceServers[0],
            session);
    }
};

avatarManager.OnSessionAvatarConnecting += (connecting) =>
{
    avatarClient.AvatarConnecting(connecting.ServerSdp);
};

// Subscribe to video/audio frames
avatarClient.OnVideoFrameReceived += (remote, ssrc, frame, format, timestamp) =>
{
    // Handle H.264 video frames
};
avatarClient.OnAudioFrameReceived += (audioData, timestamp) =>
{
    // Handle Opus audio frames
};

// Start session with handlers
var session = await client.StartAgentSessionAsync(
    projectName,
    agentId,
    options,
    new MessageHandlerManagerBase[] { serverManager, avatarManager });

// Send audio data
await session.SendInputAudioAsync(audioBytes);
```

### Using IAsyncEnumerable Pattern (Alternative to Event Handlers)

```csharp
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates;

var client = new VoiceLiveClient(endpoint, new AzureKeyCredential(apiKey));
var session = await client.StartSessionAsync("gpt-4o");

// Use modern async enumerable pattern
await foreach (var update in session.GetUpdatesAsync())
{
    switch (update)
    {
        case SessionUpdateResponseAudioDelta audio:
            // AudioData property provides decoded bytes
            var audioBytes = audio.AudioData;
            // Or use Delta property for base64 string
            break;

        case SessionUpdateSessionCreated created:
            Console.WriteLine("Session created");
            break;

        case SessionUpdateSessionUpdated updated:
            Console.WriteLine("Session updated");
            break;

        case SessionUpdateTranscriptionCompleted transcription:
            Console.WriteLine($"Transcript: {transcription.Transcript}");
            break;

        case SessionUpdateError error:
            Console.WriteLine($"Error: {error.Code} - {error.Message}");
            break;

        case SessionUpdateInputAudioBufferSpeechStarted started:
            Console.WriteLine("Speech started");
            break;

        case SessionUpdateInputAudioBufferSpeechStopped stopped:
            Console.WriteLine("Speech stopped");
            break;

        case SessionUpdateResponseDone done:
            Console.WriteLine($"Response completed: {done.Status}");
            break;
    }
}
```

### VoiceLiveSessionOptions

Configure session behavior with `VoiceLiveSessionOptions`:

```csharp
// Create with defaults (Japanese voice, noise reduction, etc.)
var options = VoiceLiveSessionOptions.CreateDefault();

// Or create with specific Azure voice
var options = VoiceLiveSessionOptions.CreateWithAzureVoice(
    "en-US-JennyNeural",
    "azure-standard");

// Or create minimal configuration
var options = VoiceLiveSessionOptions.CreateMinimal();

// Customize options
options.Model = "gpt-4o";
options.Modalities = new[] { "text", "audio" };
options.InputAudioFormat = "pcm16";
options.OutputAudioFormat = "pcm16";
options.InputAudioSamplingRate = 24000;
options.Voice = new Voice
{
    Name = "ja-JP-Nanami:DragonHDLatestNeural",
    Type = "azure-standard"
};
options.TurnDetection = new TurnDetection
{
    Type = "server_vad",
    Threshold = 0.5f,
    SilenceDurationMs = 500,
    CreateResponse = true
};
options.InputAudioNoiseReduction = new AudioInputAudioNoiseReductionSettings
{
    Type = "azure_deep_noise_suppression"
};
options.Animation = new Animation
{
    Outputs = new[] { "viseme_id" }
};
options.Instructions = "You are a helpful AI assistant.";
options.Temperature = 0.7f;
```

### Session Methods

Key methods available on `VoiceLiveSession`:

```csharp
// Audio input
await session.SendInputAudioAsync(byte[] audioData);
await session.SendInputAudioAsync(Stream audioStream, int chunkSize);
await session.CommitInputAudioAsync();
await session.ClearInputAudioAsync();

// Text input and function calls
await session.SendUserMessageAsync("Hello!");
await session.SendFunctionResultAsync(callId, result);

// Response control
await session.CreateResponseAsync();
await session.CancelResponseAsync();

// Session configuration
await session.ConfigureSessionAsync(options);

// Audio output management
await session.ClearStreamingAudioAsync();  // Clear server-side buffer
session.ClearAudioQueue();                  // Clear local queue
bool hasAudio = session.TryDequeueAudio(out byte[] audioData);
session.EnqueueAudio(audioData);

// State inspection
bool connected = session.IsConnected;
int queueCount = session.AudioQueueCount;
WebSocketState state = session.State;
```

### Core Classes

| Class | Description |
|-------|-------------|
| `VoiceLiveClient` | Entry point for creating VoiceLive sessions |
| `VoiceLiveSession` | Manages WebSocket connection and message handling |
| `VoiceLiveSessionOptions` | Configuration options for session behavior |
| `VoiceLiveClientOptions` | Configuration options for client behavior |
| `ServerMessageHandlerManager` | Event-based server message processing |
| `AvatarMessageHandlerManager` | Avatar-specific message handling |

### Avatar Classes

| Class | Description |
|-------|-------------|
| `AvatarClient` | WebRTC video streaming via SIPSorcery |
| `AvatarVideoStreamer` | H.264 video and Opus audio processing |
| `H264StreamReconstructor` | SPS/PPS header injection for stream continuity |
| `H264StreamAnalyzer` | NAL unit analysis and debugging |

### SessionUpdate Types (for IAsyncEnumerable pattern)

| Type | Description |
|------|-------------|
| `SessionUpdateSessionCreated` | Session initialization confirmed |
| `SessionUpdateSessionUpdated` | Session configuration updated |
| `SessionUpdateResponseAudioDelta` | Audio chunk received (base64 encoded) |
| `SessionUpdateError` | Error from server |
| `SessionUpdateInputAudioBufferSpeechStarted` | Speech detection started |
| `SessionUpdateInputAudioBufferSpeechStopped` | Speech detection stopped |
| `SessionUpdateTranscriptionCompleted` | Speech-to-text result |
| `SessionUpdateResponseDone` | Response completed |
| `SessionUpdateConversationItemCreated` | Conversation item created |
| `SessionUpdateResponseOutputItemDone` | Output item completed |
| `SessionUpdateUnknown` | Unrecognized message type |

### Key Events (ServerMessageHandlerManager)

| Event | Description |
|-------|-------------|
| `OnAudioDeltaReceived` | Audio data received |
| `OnTranscriptionReceived` | Speech-to-text completed |
| `OnSessionUpdateReceived` | Session configuration updated |
| `OnSessionCreatedReceived` | Session created |
| `OnErrorReceived` | Error occurred |
| `OnInputAudioBufferSpeechStartedReceived` | Speech started |
| `OnInputAudioBufferSpeechStoppedReceived` | Speech stopped |
| `OnResponseDoneReceived` | Response completed |
| `OnResponseAnimationVisemeDeltaReceived` | Viseme animation data |

## Migration from Legacy API

The old `AIModelClient` and `AIAgentClient` classes in the `VoiceLiveAPI.Avatars` namespace are deprecated.

| Old (Obsolete) | New |
|----------------|-----|
| `AIModelClient` | `VoiceLiveClient.StartSessionAsync()` |
| `AIAgentClient` | `VoiceLiveClient.StartAgentSessionAsync()` |
| `VoiceLiveAPI.Avatars.AvatarMessageHandlerManager` | `VoiceLiveAPI.Core.AvatarMessageHandlerManager` |
| `VoiceLiveAPI.Avatars.SessionAvatarConnecting` | `VoiceLiveAPI.Core.Models.AvatarConnecting` |

## License

This project is licensed under the Boost Software License 1.0 - see the [LICENSE](LICENSE) file for details.

## Knowledge

- [Microsoft Foundry documentation(Microsoft Learn)](https://learn.microsoft.com/ja-jp/azure/ai-foundry/?view=foundry-classic&wt.mc_id=WDIT-MVP-5003104)
- [Quickstart: Create a voice live real-time voice agent with Microsoft Foundry Agent Service(Microsoft Learn)](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/voice-live-agents-quickstart?toc=%2Fazure%2Fai-foundry%2Ftoc.json&bc=%2Fazure%2Fai-foundry%2Fbreadcrumb%2Ftoc.json&view=foundry-classic&preserve-view=true&tabs=windows%2Ckeyless&pivots=ai-foundry-portal&wt.mc_id=WDIT-MVP-5003104)
- [How to use the Voice live API(Microsoft Learn)](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/voice-live-how-to?wt.mc_id=WDIT-MVP-5003104)

## Stats

![GitHub stars](https://img.shields.io/github/stars/TakahiroMiyaura/VoiceLiveAPISamples?style=social)
![GitHub forks](https://img.shields.io/github/forks/TakahiroMiyaura/VoiceLiveAPISamples?style=social)
![GitHub issues](https://img.shields.io/github/issues/TakahiroMiyaura/VoiceLiveAPISamples)
![GitHub pull requests](https://img.shields.io/github/issues-pr/TakahiroMiyaura/VoiceLiveAPISamples)
