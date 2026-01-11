# Azure Voice Live API Console Application

[![License: BSL-1.0](https://img.shields.io/badge/License-BSL--1.0-blue.svg)](https://opensource.org/licenses/BSL-1.0)

A .NET 8 console application and reusable client library for real-time voice conversation with Azure AI Foundry's Voice Live API. Supports **AI Model mode**, **AI Agent mode**, and **Avatar mode** with microphone input, speaker output, and real-time video streaming.

[![Foundry VoiceLiveAPI AvatarDemo](https://img.youtube.com/vi/lZ5fp42zWNs/0.jpg)](https://www.youtube.com/watch?v=lZ5fp42zWNs)

## 🌟 Features

- **Triple Connection Modes**:
  - **AI Model Mode**: Direct connection to Azure AI models (GPT-4o, etc.)
  - **AI Agent Mode**: Connection to custom AI agents with specialized configurations
  - **Avatar Mode**: WebRTC video streaming with real-time H.264 video and Opus audio

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

## 🚀 Quick Start

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

**When using only EntraID login (az login, etc.), ‘AzureAIFoundry:ApiKey’ is not required.**

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

## 💻 Usage

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

#### AI Model Mode (New API - Recommended)
```csharp
using Azure;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server;

// Create client with Azure SDK credential
var client = new VoiceLiveClient(
    "https://your-endpoint.cognitiveservices.azure.com",
    new AzureKeyCredential("your-api-key"));

// Start session with model
var session = await client.StartSessionAsync("gpt-4o-realtime-preview");

// Setup event handlers
var messageHandler = new ServerMessageHandlerManager();
messageHandler.OnResponseAudioDelta += (audioData) => {
    // Handle received audio
};
session.AddMessageHandlerManager(messageHandler);

// Send audio data
await session.SendInputAudioAsync(audioBytes);
```

#### AI Agent Mode (New API - Recommended)
```csharp
using Azure;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server;

// Create client with Azure SDK credential
var client = new VoiceLiveClient(
    "https://your-endpoint.cognitiveservices.azure.com",
    new AzureKeyCredential("your-api-key"));

// Configure agent settings
client.AgentAccessToken = "your-agent-access-token";

// Start agent session
var session = await client.StartAgentSessionAsync(
    "your-project-name",
    "your-agent-id");

// Setup event handlers
var messageHandler = new ServerMessageHandlerManager();
messageHandler.OnResponseAudioDelta += (audioData) => {
    // Handle received audio
};
session.AddMessageHandlerManager(messageHandler);

// Send audio data
await session.SendInputAudioAsync(audioBytes);
```

#### Avatar Mode (New API - Recommended)
```csharp
using Azure;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars;

// Create client
var client = new VoiceLiveClient(endpoint, new AzureKeyCredential(apiKey));
var session = await client.StartAgentSessionAsync(projectName, agentId);

// Create avatar client for WebRTC
var avatarClient = new AvatarClient();

// Subscribe to video/audio frames
avatarClient.OnVideoFrameReceived += (remote, ssrc, frame, format, timestamp) => {
    // Handle H.264 video frames
};
avatarClient.OnAudioFrameReceived += (audioData, timestamp) => {
    // Handle Opus audio frames
};

// Connect avatar with session
await avatarClient.AvatarConnectAsync(iceServers, session);
```

#### Legacy API (Deprecated)
```csharp
// The legacy AIModelClient and AIAgentClient are deprecated.
// Please migrate to the new VoiceLiveClient/VoiceLiveSession API.
// See docs/MIGRATION_GUIDE.md for migration instructions.
```

## 📖 API Reference

### Core Classes (New API)

- `VoiceLiveClient`: Main entry point for creating VoiceLive sessions
- `VoiceLiveSession`: Manages WebSocket connection and message handling
- `VoiceLiveSessionOptions`: Configuration options for session behavior
- `VoiceLiveCredential`: Unified credential handling (API Key / Token)
- `ServerMessageHandlerManager`: Event-based message processing

### Avatar Classes

- `AvatarClient`: WebRTC video streaming via SIPSorcery
- `AvatarVideoStreamer`: H.264 video and Opus audio processing
- `H264StreamReconstructor`: SPS/PPS header injection for stream continuity
- `H264StreamAnalyzer`: NAL unit analysis and debugging
- `AvatarMessageHandlerManager`: Avatar-specific message handling

### Key Session Methods

- `StartSessionAsync(model)`: Start a model session
- `StartAgentSessionAsync(projectName, agentId)`: Start an agent session
- `SendInputAudioAsync(audioData)`: Send audio to the session
- `ConfigureSessionAsync(options)`: Update session configuration
- `ClearAudioQueue()`: Clear local audio output queue

### Key Events (via ServerMessageHandlerManager)

- `OnResponseAudioDelta`: Handles incoming audio data
- `OnSessionCreated`: Session creation confirmation
- `OnSessionUpdated`: Session configuration update confirmation
- `OnTranscriptionCompleted`: Speech-to-text result
- `OnError`: Error handling

### Legacy Classes (Deprecated)

- `AIModelClient`: Use `VoiceLiveClient.StartSessionAsync()` instead
- `AIAgentClient`: Use `VoiceLiveClient.StartAgentSessionAsync()` instead

## 📜 License

This project is licensed under the Boost Software License 1.0 - see the [LICENSE](LICENSE) file for details.

## Knowledge

- [Microsoft Foundry documentation(Microsoft Learn)](https://learn.microsoft.com/ja-jp/azure/ai-foundry/?view=foundry-classic&wt.mc_id=WDIT-MVP-5003104)
- [Quickstart: Create a voice live real-time voice agent with Microsoft Foundry Agent Service(Microsoft Learn)](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/voice-live-agents-quickstart?toc=%2Fazure%2Fai-foundry%2Ftoc.json&bc=%2Fazure%2Fai-foundry%2Fbreadcrumb%2Ftoc.json&view=foundry-classic&preserve-view=true&tabs=windows%2Ckeyless&pivots=ai-foundry-portal&wt.mc_id=WDIT-MVP-5003104)
- [How to use the Voice live API(Microsoft Learn)](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/voice-live-how-to?wt.mc_id=WDIT-MVP-5003104)

## 📈 Stats

![GitHub stars](https://img.shields.io/github/stars/TakahiroMiyaura/VoiceLiveAPISamples?style=social)
![GitHub forks](https://img.shields.io/github/forks/TakahiroMiyaura/VoiceLiveAPISamples?style=social)
![GitHub issues](https://img.shields.io/github/issues/TakahiroMiyaura/VoiceLiveAPISamples)
![GitHub pull requests](https://img.shields.io/github/issues-pr/TakahiroMiyaura/VoiceLiveAPISamples)
