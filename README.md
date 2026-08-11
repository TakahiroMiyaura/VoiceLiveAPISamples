# Azure Voice Live API Console Application

[![License: BSL-1.0](https://img.shields.io/badge/License-BSL--1.0-blue.svg)](https://opensource.org/licenses/BSL-1.0)

A .NET 10 console application and reusable client library for real-time voice conversation with Azure AI Foundry's Voice Live API. Supports **AI Model mode**, **AI Agent mode**, and **Avatar mode** with microphone input, speaker output, and real-time video streaming.

[![Foundry VoiceLiveAPI AvatarDemo](https://img.youtube.com/vi/lZ5fp42zWNs/0.jpg)](https://www.youtube.com/watch?v=lZ5fp42zWNs)

## Features

- **Standard modes** (generally available):
  - **AI Model**: direct connection to an Azure AI model (GPT-4o and friends)
  - **AI Agent**: connection to a Foundry agent, whose prompt and tools live in the cloud
  - **Avatar**: WebRTC video streaming with H.264 video and Opus audio

- **Preview features**, exercised one at a time so a failure points at one thing:
  - **Photo avatar** (`vasa-1`): a talking head generated from a single portrait, standard or your own
  - **azure-personal voice**: your own voice, and it composes with an avatar
  - **Client-side echo cancellation reference**: interleaved mic + played audio
  - **WebRTC voice** (`/calls`), **WebSocket avatar video** (`response.video.delta`)
  - **MCP servers**, **Foundry agent as a tool**, **parallel tool calls**, **smart end-of-turn**,
    **auto-truncation**, **interim response**, **proactive greeting**, **native voice**
  - The list is scoped to the API version you pick, so you only see what that version actually has

- **Multiple Authentication Methods**:
  - API Key authentication
  - Azure SDK credential (AzureKeyCredential, TokenCredential)
  - Bearer Token (for Unity/non-Azure environments)

- **Modern Async Patterns**:
  - Event-based handlers via `ServerMessageHandlerManager`
  - IAsyncEnumerable stream pattern via `GetUpdatesAsync()`

## Sample Applications

This repository contains two console application samples:

**Which one to start with**: if you want the stable feature set with the least machinery, use
**VoiceLiveSDKConsoleApp**. If you want to see a preview feature exercised on its own, use
**VoiceLiveConsoleApp**, which talks to the wire directly and can select `2026-06-01-preview`.

### VoiceLiveConsoleApp (Custom WebSocket Implementation)

A console application utilizing "VoiceLiveAPI.Core," a custom WebSocket library built from scratch based on the Foundry Tools Voice Live API specifications.

- **Direct WebSocket control**: Full control over WebSocket communication
- **Custom message handling**: Event-based handlers via `ServerMessageHandlerManager`
- **Preview features**: one at a time, scoped to the API version you pick

### VoiceLiveSDKConsoleApp (Azure.AI.VoiceLive SDK)

A console application that uses the official **Azure.AI.VoiceLive SDK** package, covering the four
stable patterns: AI Model, AI Agent, and Avatar on either backend.

- **Official SDK**: Uses Microsoft's official Azure.AI.VoiceLive NuGet package
- **Simplified API**: `VoiceLiveClient` and `VoiceLiveSession` classes from the SDK
- **IAsyncEnumerable pattern**: Modern async streaming via `session.GetUpdatesAsync()`
- **Avatar support**: Uses existing VoiceLiveAPI.Avatars for WebRTC video streaming

| Feature | VoiceLiveConsoleApp | VoiceLiveSDKConsoleApp |
|---------|---------------------|------------------------|
| WebSocket Implementation | Custom (VoiceLiveAPI.Core) | Azure.AI.VoiceLive SDK |
| Session Management | VoiceLiveSession (Core) | VoiceLiveSession (SDK) |
| Message Handling | ServerMessageHandlerManager events | IAsyncEnumerable pattern |
| API version | up to `2026-06-01-preview` | up to `2026-01-01-preview` (SDK limit) |
| Preview features | 14, chosen from a menu | — |
| Avatar Video Streaming | WebRTC and WebSocket | WebRTC |
| Authentication | API Key / Entra ID | API Key / Entra ID |

#### Running VoiceLiveSDKConsoleApp

```powershell
# Build the SDK console application
PS D:\hoge\VoiceLiveAPISamples > dotnet build src\VoiceLiveSDKConsoleApp

# Run the application
PS D:\hoge\VoiceLiveAPISamples > dotnet run --project src/VoiceLiveSDKConsoleApp
```

> [!NOTE]
> VoiceLiveSDKConsoleApp uses the same user secrets configuration as VoiceLiveConsoleApp.
> If you have already configured user secrets for VoiceLiveConsoleApp, you can use the same configuration.

## Required Packages

### VoiceLiveConsoleApp

| Package Name                                 | Version         | Purpose                    |
|----------------------------------------------|-----------------|----------------------------|
| Microsoft.Extensions.Configuration           | 10.0.10           | Configuration management   |
| Microsoft.Extensions.Configuration.UserSecrets | 10.0.10        | Secure configuration       |
| Microsoft.Extensions.Logging                | 10.0.10           | Logging infrastructure     |
| System.Text.Json                             | 10.0.3          | JSON serialization         |
| NAudio                                       | 2.3.0            | Cross-platform audio      |
| SIPSorcery                                   | 10.0.9          | WebRTC implementation      |
| SIPSorceryMedia.Abstractions                | 10.0.9          | Media format abstractions |
| Concentus                                    | 2.2.2           | Opus audio codec           |
| FFMpegCore                                   | 5.1.0           | FFmpeg integration         |
| CliWrap                                      | 3.6.6           | Command line process wrapper |

> [!NOTE]
> `Azure.Identity` への直接参照は削除しました。`DefaultAzureCredential` などの資格情報型は `Azure.Core` (1.57.0) が提供します（`Azure.Identity` を併用すると `Azure.Identity.DefaultAzureCredential` が二重定義され CS0433 衝突するため）。

### VoiceLiveSDKConsoleApp (Additional)

| Package Name                                 | Version         | Purpose                    |
|----------------------------------------------|-----------------|----------------------------|
| Azure.AI.VoiceLive                           | 1.1.0 (GA)      | Official Azure VoiceLive SDK |

> [!NOTE]
> VoiceLiveSDKConsoleApp also uses NAudio, Concentus, and SIPSorcery packages for audio/video processing
> (the avatar media pipeline lives in the shared `VoiceLiveAPI.Avatars.Streaming` library).
>
> **Agent mode requires Microsoft Entra ID authentication** (`DefaultAzureCredential`); the Voice Live
> service does not accept API keys for agent invocation. AI Model mode supports API key auth.

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

Only the first two are needed to talk to a model. Add the agent ones when you want AI Agent mode.

```powershell
PS D:\hoge\VoiceLiveAPISamples> dotnet user-secrets init --project src\VoiceLiveConsoleApp

# Required
PS ...> dotnet user-secrets set "VoiceLiveAPI:AzureEndpoint" "<your Azure AI Services Endpoint>" --project src\VoiceLiveConsoleApp
PS ...> dotnet user-secrets set "Identity:AzureEndpoint" "https://ai.azure.com/.default" --project src\VoiceLiveConsoleApp

# API key authentication (skip it if you only sign in with Entra ID / az login)
PS ...> dotnet user-secrets set "AzureAIFoundry:ApiKey" "<Azure AI Foundry API Key>" --project src\VoiceLiveConsoleApp

# AI Agent mode (Entra ID only — an API key is not accepted for agent sessions)
PS ...> dotnet user-secrets set "AzureAIFoundry:AgentName" "<your agent name>" --project src\VoiceLiveConsoleApp
PS ...> dotnet user-secrets set "AzureAIFoundry:AgentProjectName" "<your Azure AI Foundry Project Name>" --project src\VoiceLiveConsoleApp
```

> [!IMPORTANT]
> Agent sessions connect by **name** (`AgentName` + `AgentProjectName`). The older `AzureAIFoundry:AgentId`
> and `AzureAIFoundry:AgentAccessToken` pair is for the classic agent connection, which **retires on
> 2026-08-31** — set those two only if you are still using it.

Every one of these can also be given as an environment variable, which is usually easier for a
machine you already have set up: see [Configuration](#configuration) below, or run
`dotnet run --project src/VoiceLiveConsoleApp -- --help` to list every setting with its current value.

3. Build the console application.
```powershell
PS D:\hoge\VoiceLiveAPISamples > dotnet build src\VoiceLiveConsoleApp
```

4. Run the application.
```powershell
PS D:\hoge\VoiceLiveAPISamples > dotnet run --project src/VoiceLiveConsoleApp
```


### Configuration

Every setting can go in user secrets. What differs is how you override it, and that follows from what the
setting is for:

| Category | Examples | Override with |
|---|---|---|
| **Connection and credentials** — fixed per environment | endpoint, API key, agent name/project, model | **environment variable** (`VOICELIVE_ENDPOINT`, ...) |
| **Feature inputs** — change per run | personal voice, photo avatar, MCP server, greeting | **command-line argument** (`--photo-avatar`, ...) |
| **Diagnostics** — one run only | wire trace, log level | **command-line argument** (`--wire-debug`, ...) |

Values resolve as **default -> user secrets -> environment variable -> command-line argument**, so the
narrower the scope, the higher it wins. Flags accept `1`, `true`, `yes` or `on` from any source, or the
bare switch on the command line.

To list every setting, how it can be supplied, and where its value is currently coming from:

```powershell
PS D:\hoge\VoiceLiveAPISamples > dotnet run --project src/VoiceLiveConsoleApp -- --help
```

Examples:

```powershell
PS ...> dotnet run --project src/VoiceLiveConsoleApp -- --photo-avatar ren --personal-voice <speaker-profile-guid>
PS ...> dotnet run --project src/VoiceLiveConsoleApp -- --wire-debug
PS ...> dotnet run --project src/VoiceLiveConsoleApp -- --log-level Information --resolve-agent-model
```
## Usage

### Console Application

The menu asks what you want first: the stable feature set, or one of the preview additions.

> [!CAUTION]
> The API key can only be used in AI Model Mode.
> For AI Agent Mode and Avatar Mode, please use Entra ID authentication (DefaultAzureCredential).
> Before using Entra ID authentication, ensure you have logged in using the Azure CLI (`az login`).

```
Choose:
1. Standard features  (GA — talk to a model, an agent, or an avatar)
2. Preview features   (try one addition of a preview API version)
Enter your choice (1 or 2): 1

Choose a standard mode:
1. AI Model   (talk to a model)
2. AI Agent   (talk to a Foundry agent)
3. Avatar     (model or agent, with WebRTC video)
Enter your choice (1-3): 3

Choose Avatar session backend:
1. Agent (Foundry agent, Entra ID required)
2. Model (direct model session, enables image input)
Enter your choice (1 or 2) [default: 1]: 1

Choose authentication method:
1. API Key
2. Entra ID (DefaultAzureCredential)
Enter your choice (1 or 2): 2

Ready for conversation!
Commands:
- 'R' record (auto-stops when you finish speaking)
- 'X' send text
- 'I' send an image
- 'Q' quit
  (diagnostics: 'S' status, 'C' clear audio, 'P' playback, 'T' reconnect, 'M' switch mode)
```

**Recording Auto-Stop**: recording stops on its own once the service detects you have finished
speaking, so background noise doesn't cut into the answer. Press 'R' again to stop it yourself.

Choosing **2. Preview features** asks for the API version first — that scopes the list to what the
version actually supports — and then for the one feature to exercise:

```
Choose API version (VoiceLiveConsoleApp targets preview wire versions):
1. 2026-01-01-preview
2. 2026-06-01-preview
Enter your choice (1-2) [default: 2]: 2

Choose a 2026-06-01-preview feature to check (runs as an AI Model session):
 1. Auto-truncation (turn_detection.auto_truncate on barge-in)
 2. WebRTC voice (RTP audio over /calls instead of the WebSocket)
 3. Avatar with WebSocket video (response.video.delta, no SDP/ICE)
 4. Photo avatar (talking head generated from a single image by vasa-1)
 5. Smart end-of-turn detection
 ...
```

Each feature prints how to try it once the session is up.

#### Avatar Mode Features
- **Real-time Video**: H.264 video streaming with automatic SPS/PPS reconstruction
- **Audio Integration**: Opus audio capture and MPEGTS multiplexing
- **FFplay Integration**: the video window opens automatically once frames arrive
- **Backend**: an avatar is an output layer, so it runs on either an agent or a model session.
  The model backend also enables image input.

#### Photo avatar

A photo avatar is a talking head generated from a single portrait by `vasa-1`, rather than a
pre-rendered character. Pick it under **Preview features**; the standard character is `sakura`.

```powershell
PS ...> dotnet run --project src/VoiceLiveConsoleApp -- --photo-avatar ren
```

To use one you created yourself in Microsoft Foundry (Build → Fine-tune → AI Services →
*Azure Speech - Text to Speech Avatar* → Type = **Photo avatar**), just name it — there is no
deployment step, and a name that isn't a standard talking head is sent as a custom avatar
automatically:

```powershell
PS ...> dotnet run --project src/VoiceLiveConsoleApp -- --photo-avatar my-avatar
```

It differs from the video avatar in ways worth knowing: `vasa-1` is required, the standard
characters have no styles, no crop is needed (the frame is already a head shot), and
`video.resolution` is not honored — frames arrive at the source portrait's aspect ratio.

#### Your own voice on an avatar

Voice and avatar are independent settings, so a personal voice can drive an avatar. That gets you
"my face, my voice" from **one photo and about thirty seconds of audio**, where a custom video
avatar's voice-sync would need ten minutes of consistent studio recording.

```powershell
PS ...> dotnet run --project src/VoiceLiveConsoleApp -- --photo-avatar my-avatar --personal-voice <speaker-profile-guid>
```

> [!NOTE]
> `--personal-voice` takes the **speaker profile ID** — a GUID that currently appears only in the
> URL of the personal voice page in the portal, not the voice name and not the "Profile ID" the
> page displays.

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

### Console Classes (VoiceLiveConsoleApp)

| Class | Description |
|-------|-------------|
| `ConsoleSettings` | Every setting and how it can be supplied; also generates `--help` |
| `ConsoleMenu` | The startup prompts, returning the answers as a value |
| `AudioPipeline` | Microphone, speakers, barge-in gating, echo reference |
| `AvatarSession` | Avatar configuration, transport, and SDP/ICE negotiation |
| `AgentModelResolver` | Which model actually answered (useful behind Model Router) |
| `PreviewFeatureCatalog` | The preview features — one entry per feature |

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
