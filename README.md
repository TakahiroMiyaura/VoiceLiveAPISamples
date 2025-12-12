# Azure Voice Live API Console Application

[![License: BSL-1.0](https://img.shields.io/badge/License-BSL--1.0-blue.svg)](https://opensource.org/licenses/BSL-1.0)

A .NET 8 console application and reusable client library for real-time voice conversation with Azure AI Foundry's Voice Live API. Supports **AI Model mode**, **AI Agent mode**, and **Avatar mode** with microphone input, speaker output, and real-time video streaming.

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

#### AI Model Mode
```csharp
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients;

var client = new AIModelClient(token);
client.OnResponseAudioDelta += (audioData) => {
    // Handle received audio
};
await client.ConnectAsync();
```

#### AI Agent Mode
```csharp
var client = new AIAgentClient(token, agentProjectName, agentId);
client.OnResponseAudioDelta += (audioData) => {
    // Handle received audio
};
await client.ConnectAsync();
```

#### Avatar Mode
```csharp
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars;

var avatarClient = new AvatarClient(token);
var videoStreamer = new AvatarVideoStreamer(avatarClient, logger);

// Subscribe to video/audio frames
avatarClient.OnVideoFrameReceived += (remote, ssrc, frame, format) => {
    // Handle H.264 video frames
};
avatarClient.OnAudioFrameReceived += (audioData) => {
    // Handle Opus audio frames
};

// Start video streaming
videoStreamer.StartStreaming();
await avatarClient.ConnectAsync();
```

## 📖 API Reference

### Core Classes

- `AIModelClient`: For direct AI model connections
- `AIAgentClient`: For AI agent connections  
- `AvatarClient`: For WebRTC video streaming connections
- `AvatarVideoStreamer`: H.264 video and Opus audio processing
- `H264StreamReconstructor`: SPS/PPS header injection for stream continuity
- `H264StreamAnalyzer`: NAL unit analysis and debugging
- `VoiceLiveHandlerBase`: Base handler for custom event processing

### Key Events

- `OnResponseAudioDelta`: Handles incoming audio data
- `OnVideoFrameReceived`: Handles H.264 video frames (Avatar mode)
- `OnAudioFrameReceived`: Handles Opus audio frames (Avatar mode)
- `OnSessionCreated`: Handles session establishment

### Avatar Processing Classes

- `H264StreamAnalyzer`: NAL unit analysis and debugging
- `AvatarMessageHandlerManager`: Avatar-specific message handling

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
