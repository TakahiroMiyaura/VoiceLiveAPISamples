# Azure Voice Live API Console Application

[![.NET Build and Test](https://github.com/TakahiroMiyaura/VoiceLiveAPISamples/actions/workflows/dotnet.yml/badge.svg)](https://github.com/TakahiroMiyaura/VoiceLiveAPISamples/actions/workflows/dotnet.yml)
[![Release](https://github.com/TakahiroMiyaura/VoiceLiveAPISamples/actions/workflows/release.yml/badge.svg)](https://github.com/TakahiroMiyaura/VoiceLiveAPISamples/releases)
[![NuGet Version](https://img.shields.io/nuget/v/Azure.VoiceLive.API)](https://www.nuget.org/packages/Azure.VoiceLive.API)
[![License: BSL-1.0](https://img.shields.io/badge/License-BSL--1.0-blue.svg)](https://opensource.org/licenses/BSL-1.0)

A .NET 8 console application and reusable client library for real-time voice conversation with Azure AI Foundry's Voice Live API. Supports both **AI Model mode** and **AI Agent mode** with microphone input and speaker output.

## 🌟 Features

- **Dual Connection Modes**:
  - **AI Model Mode**: Direct connection to Azure AI models (GPT-4o, etc.)
  - **AI Agent Mode**: Connection to custom AI agents with specialized configurations
- **Real-time Audio Processing**: Bidirectional audio streaming with low latency
- **Voice Activity Detection**: Azure's semantic VAD for natural conversation flow
- **Audio Enhancement**: Built-in noise suppression and echo cancellation
- **Cross-Platform**: Windows, Linux, and macOS support
- **Modular Design**: Reusable `VoiceLiveApiClient` class for integration
- **Runtime Mode Switching**: Change between AI Model and AI Agent modes dynamically

## Required Packages

| Package Name                                 | Version         |
|----------------------------------------------|-----------------|
| Microsoft.Extensions.Logging                | 9.0.8           |
| NAudio                                       | 2.2.1           |
| System.Text.Json                             | 9.0.7           |
| Azure.Identity                               | 1.14.2          |
| Microsoft.Extensions.Configuration           | 9.0.7           |
| Microsoft.Extensions.Configuration.UserSecrets | 9.0.7        |



## 🚀 Quick Start

1. Access the Azure portal and add the following resources:
* Azure AI Foundry & Project
* Something AI Agent (optional, for AI Agent mode)
   Make sure to note the **Project Endpoint** and **API KEY** if using AI Agent mode.

2. Clone the sample project.

```powershell
PS C:\hoge> git clone https://github.com/TakahiroMiyaura/VoiceLiveAPISamples.git
PS C:\hoge> cd VoiceLiveAPISamples
```

3. Register the Azure AI Foundry endpoint.

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

5. Build the console application.
```powershell
PS D:\hoge\VoiceLiveAPISamples > dotnet build src\VoiceLiveConsoleApp
```

6. Run the application.
```powershell
PS D:\hoge\VoiceLiveAPISamples > .\src\VoiceLiveConsoleApp\bin\Debug\net8.0\VoiceLiveConsoleApp.exe
```

## 💻 Usage

### Console Application

Run the application and choose between AI Model or AI Agent mode:

```
Choose connection mode:
1. AI Model Mode
2. AI Agent Mode

Commands:
- Press 'R' to start/stop recording
- Press 'P' to start/stop playback
- Press 'M' to switch modes
- Press 'Q' to quit
```

### Library Usage

#### AI Model Mode
```csharp
using VoiceLiveAPI;

var client = new AIModelClient(endpoint, credentials);
client.OnAudioDataReceived += (audioData) => {
    // Handle received audio
};
await client.ConnectAsync();
```

#### AI Agent Mode
```csharp
var client = new AIAgentClient(endpoint, credentials, agentConfig);
client.OnAudioDataReceived += (audioData) => {
    // Handle received audio
};
await client.ConnectAsync();
```

## 📖 API Reference

### Core Classes

- `AIModelClient`: For direct AI model connections
- `AIAgentClient`: For AI agent connections
- `VoiceLiveHandlerBase`: Base handler for custom event processing

### Key Events

- `OnAudioDataReceived`: Handles incoming audio data
- `OnResponseReceived`: Handles text responses
- `OnSessionCreated`: Handles session establishment

## 📜 License

This project is licensed under the Boost Software License 1.0 - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Azure AI Foundry Team** - For the Voice Live API
- **NAudio Contributors** - For cross-platform audio support
- **Community Contributors** - For bug reports, feature requests, and improvements

## 📈 Stats

![GitHub stars](https://img.shields.io/github/stars/TakahiroMiyaura/VoiceLiveAPISamples?style=social)
![GitHub forks](https://img.shields.io/github/forks/TakahiroMiyaura/VoiceLiveAPISamples?style=social)
![GitHub issues](https://img.shields.io/github/issues/TakahiroMiyaura/VoiceLiveAPISamples)
![GitHub pull requests](https://img.shields.io/github/issues-pr/TakahiroMiyaura/VoiceLiveAPISamples)
