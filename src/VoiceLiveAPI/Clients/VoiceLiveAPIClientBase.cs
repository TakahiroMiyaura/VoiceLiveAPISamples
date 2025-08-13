// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.InputAudioBuffers;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Sessions;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.Conversations;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.Conversations.Items;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.InputAudioBuffers;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.Responses.FunctionCallArguments;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.Responses.Texts;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Conversations.Items;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.InputAudioBuffers;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.OutputAudioBuffers;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses.Audios;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses.AudioTranscripts;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses.ContentParts;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses.OutputItems;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Sessions;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Conversations.Items;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.InputAudioBuffers;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.Audios;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.AudioTranscripts;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.ContentParts;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.OutputItems;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Sessions;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Conversations.Items;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Conversations;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Conversations.Items;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.InputAudioBuffers;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.OutputAudioBuffers;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Responses.FunctionCallArguments;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Responses.Texts;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients;

/// <summary>
///     Abstract base class for Azure AI VoiceInfo Live API clients.
///     Provides common functionality for WebSocket communication, audio processing, and message handling.
/// </summary>
public abstract class VoiceLiveAPIClientBase : IDisposable
{
    /// <summary>
    ///     Authentication methods supported by the VoiceInfo Live API client.
    /// </summary>
    public enum AuthenticationMethod
    {
        /// <summary>
        ///     API key-based authentication.
        /// </summary>
        ApiKey,

        /// <summary>
        ///     Microsoft Entra ID (keyless) authentication.
        /// </summary>
        EntraId
    }

    #region Constructor

    /// <summary>
    ///     Initializes a new instance of the VoiceLiveAPIClientBase.
    /// </summary>
    protected VoiceLiveAPIClientBase()
    {
        WebSocket = new ClientWebSocket();
        CancellationTokenSource = new CancellationTokenSource();
        ReceiveTask = Task.CompletedTask;
        AccessToken = string.Empty;
    }

    #endregion


    #region Private Fields

    /// <summary>
    ///     The WebSocket client used for communication.
    /// </summary>
    protected ClientWebSocket WebSocket;

    /// <summary>
    ///     The cancellation token source for managing task cancellation.
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource;

    /// <summary>
    ///     A queue for storing audio output data.
    /// </summary>
    protected readonly ConcurrentQueue<byte[]> AudioOutputQueue = new();

    /// <summary>
    ///     Dictionary for storing registered message handlers.
    /// </summary>
    private readonly Dictionary<string, IVoiceLiveHandler> messageHandlers = new();

    /// <summary>
    ///     The task responsible for receiving data.
    /// </summary>
    protected Task ReceiveTask;

    /// <summary>
    ///     The access token used for authentication.
    /// </summary>
    protected string AccessToken;

    #endregion

    #region Public Properties

    /// <summary>
    ///     Gets the Azure AI endpoint URL.
    /// </summary>
    public string Endpoint { get; protected set; } = string.Empty;

    /// <summary>
    ///     Gets the API version.
    /// </summary>
    public string ApiVersion { get; protected set; } = "2025-05-01-preview";

    /// <summary>
    ///     Gets the API key for authentication.
    /// </summary>
    public string ApiKey { get; protected set; } = string.Empty;

    /// <summary>
    ///     Gets the authentication method.
    /// </summary>
    public AuthenticationMethod AuthMethod { get; protected set; } = AuthenticationMethod.ApiKey;

    /// <summary>
    ///     Gets the token credential for Entra ID authentication.
    /// </summary>
    protected TokenCredential? TokenCredential { get; set; }

    /// <summary>
    ///     Gets the token request context for Entra ID authentication.
    /// </summary>
    protected TokenRequestContext RequestContext { get; set; }

    /// <summary>
    ///     Gets or sets the logger instance.
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    ///     Gets or sets the audio sampling rate in Hz (default: 24000).
    /// </summary>
    public int SamplingRate { get; set; } = 24000;

    #endregion

    #region Events

    /// <summary>
    ///     Event fired when an audio delta response is received.
    /// </summary>
    public event Action<ResponseAudioDelta>? OnAudioDeltaReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseAudioDeltaHandler.EventType, out var handler))
            {
                ((ResponseAudioDeltaHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseAudioDeltaHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseAudioDeltaHandler.EventType, out var handler))
            {
                ((ResponseAudioDeltaHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseAudioDeltaHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a transcription is received.
    /// </summary>
    public event Action<ConversationItemInputAudioTranscriptionCompleted>? OnTranscriptionReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ConversationItemInputAudioTranscriptionCompletedHandler.EventType,
                    out var handler))
            {
                ((ConversationItemInputAudioTranscriptionCompletedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ConversationItemInputAudioTranscriptionCompletedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ConversationItemInputAudioTranscriptionCompletedHandler.EventType,
                    out var handler))
            {
                ((ConversationItemInputAudioTranscriptionCompletedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseAudioDeltaHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a session update response is received.
    /// </summary>
    public event Action<ServerSessionUpdated>? OnSessionUpdateReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ServerSessionUpdateHandler.EventType, out var handler))
            {
                ((ServerSessionUpdateHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ServerSessionUpdateHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ServerSessionUpdateHandler.EventType, out var handler))
            {
                ((ServerSessionUpdateHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseAudioDeltaHandler.");
            }
        }
    }

    // Server Events
    /// <summary>
    ///     Event fired when a conversation created message is processed.
    /// </summary>
    public event Action<ConversationCreatedMessage>? OnConversationCreatedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ConversationCreatedHandler.EventType, out var handler))
            {
                ((ConversationCreatedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ConversationCreatedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ConversationCreatedHandler.EventType, out var handler))
            {
                ((ConversationCreatedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ConversationCreatedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a conversation Item created message is processed.
    /// </summary>
    public event Action<ConversationItemCreated>? OnConversationItemCreatedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ConversationItemCreatedHandler.EventType, out var handler))
            {
                ((ConversationItemCreatedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ConversationItemCreatedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ConversationItemCreatedHandler.EventType, out var handler))
            {
                ((ConversationItemCreatedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ConversationItemCreatedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a conversation Item retrieved message is processed.
    /// </summary>
    public event Action<ConversationItemRetrievedMessage>? OnConversationItemRetrievedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ConversationItemRetrievedHandler.EventType, out var handler))
            {
                ((ConversationItemRetrievedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ConversationItemRetrievedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ConversationItemRetrievedHandler.EventType, out var handler))
            {
                ((ConversationItemRetrievedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ConversationItemRetrievedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a conversation Item deleted message is processed.
    /// </summary>
    public event Action<ConversationItemDeletedMessage>? OnConversationItemDeletedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ConversationItemDeletedHandler.EventType, out var handler))
            {
                ((ConversationItemDeletedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ConversationItemDeletedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ConversationItemDeletedHandler.EventType, out var handler))
            {
                ((ConversationItemDeletedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ConversationItemDeletedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a conversation Item input audio transcription failed message is processed.
    /// </summary>
    public event Action<ConversationItemInputAudioTranscriptionFailedMessage>?
        OnConversationItemInputAudioTranscriptionFailedReceived
        {
            add
            {
                if (messageHandlers.TryGetValue(ConversationItemInputAudioTranscriptionFailedHandler.EventType,
                        out var handler))
                {
                    ((ConversationItemInputAudioTranscriptionFailedHandler)handler).OnProcessMessage += value;
                }
                else
                {
                    var h = new ConversationItemInputAudioTranscriptionFailedHandler();
                    h.OnProcessMessage += value;
                    RegisterMessageHandler(h);
                }
            }
            remove
            {
                if (messageHandlers.TryGetValue(ConversationItemInputAudioTranscriptionFailedHandler.EventType,
                        out var handler))
                {
                    ((ConversationItemInputAudioTranscriptionFailedHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Handler not registered for ConversationItemInputAudioTranscriptionFailedHandler.");
                }
            }
        }

    /// <summary>
    ///     Event fired when a conversation Item truncated message is processed.
    /// </summary>
    public event Action<ConversationItemTruncatedMessage>? OnConversationItemTruncatedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ConversationItemTruncatedHandler.EventType, out var handler))
            {
                ((ConversationItemTruncatedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ConversationItemTruncatedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ConversationItemTruncatedHandler.EventType, out var handler))
            {
                ((ConversationItemTruncatedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ConversationItemTruncatedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when an error message is processed.
    /// </summary>
    public event Action<Error>? OnErrorReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ErrorHandler.EventType, out var handler))
            {
                ((ErrorHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ErrorHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ErrorHandler.EventType, out var handler))
            {
                ((ErrorHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ErrorHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when an input audio buffer cleared message is processed.
    /// </summary>
    public event Action<InputAudioBufferClearedMessage>? OnInputAudioBufferClearedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(InputAudioBufferClearedHandler.EventType, out var handler))
            {
                ((InputAudioBufferClearedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new InputAudioBufferClearedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(InputAudioBufferClearedHandler.EventType, out var handler))
            {
                ((InputAudioBufferClearedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for InputAudioBufferClearedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when an input audio buffer committed message is processed.
    /// </summary>
    public event Action<InputAudioBufferCommitted>? OnInputAudioBufferCommittedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(InputAudioBufferCommittedHandler.EventType, out var handler))
            {
                ((InputAudioBufferCommittedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new InputAudioBufferCommittedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(InputAudioBufferCommittedHandler.EventType, out var handler))
            {
                ((InputAudioBufferCommittedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for InputAudioBufferCommittedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when an input audio buffer speech started message is processed.
    /// </summary>
    public event Action<InputAudioBufferSpeechStarted>? OnInputAudioBufferSpeechStartedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(InputAudioBufferSpeechStartedHandler.EventType, out var handler))
            {
                ((InputAudioBufferSpeechStartedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new InputAudioBufferSpeechStartedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(InputAudioBufferSpeechStartedHandler.EventType, out var handler))
            {
                ((InputAudioBufferSpeechStartedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for InputAudioBufferSpeechStartedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when an input audio buffer speech stopped message is processed.
    /// </summary>
    public event Action<InputAudioBufferSpeechStopped>? OnInputAudioBufferSpeechStoppedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(InputAudioBufferSpeechStoppedHandler.EventType, out var handler))
            {
                ((InputAudioBufferSpeechStoppedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new InputAudioBufferSpeechStoppedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(InputAudioBufferSpeechStoppedHandler.EventType, out var handler))
            {
                ((InputAudioBufferSpeechStoppedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for InputAudioBufferSpeechStoppedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when an output audio buffer cleared message is processed.
    /// </summary>
    public event Action<OutputAudioBufferClearedMessage>? OnOutputAudioBufferClearedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(OutputAudioBufferClearedHandler.EventType, out var handler))
            {
                ((OutputAudioBufferClearedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new OutputAudioBufferClearedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(OutputAudioBufferClearedHandler.EventType, out var handler))
            {
                ((OutputAudioBufferClearedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for OutputAudioBufferClearedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when an output audio buffer started message is processed.
    /// </summary>
    public event Action<OutputAudioBufferStartedMessage>? OnOutputAudioBufferStartedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(OutputAudioBufferStartedHandler.EventType, out var handler))
            {
                ((OutputAudioBufferStartedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new OutputAudioBufferStartedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(OutputAudioBufferStartedHandler.EventType, out var handler))
            {
                ((OutputAudioBufferStartedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for OutputAudioBufferStartedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when an output audio buffer stopped message is processed.
    /// </summary>
    public event Action<OutputAudioBufferStoppedMessage>? OnOutputAudioBufferStoppedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(OutputAudioBufferStoppedHandler.EventType, out var handler))
            {
                ((OutputAudioBufferStoppedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new OutputAudioBufferStoppedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(OutputAudioBufferStoppedHandler.EventType, out var handler))
            {
                ((OutputAudioBufferStoppedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for OutputAudioBufferStoppedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a rate limits updated message is processed.
    /// </summary>
    public event Action<RateLimitsUpdatedMessage>? OnRateLimitsUpdatedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(RateLimitsUpdatedHandler.EventType, out var handler))
            {
                ((RateLimitsUpdatedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new RateLimitsUpdatedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(RateLimitsUpdatedHandler.EventType, out var handler))
            {
                ((RateLimitsUpdatedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for RateLimitsUpdatedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response audio done message is processed.
    /// </summary>
    public event Action<ResponseAudioDone>? OnResponseAudioDoneReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseAudioDoneHandler.EventType, out var handler))
            {
                ((ResponseAudioDoneHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseAudioDoneHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseAudioDoneHandler.EventType, out var handler))
            {
                ((ResponseAudioDoneHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseAudioDoneHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response audio transcript delta message is processed.
    /// </summary>
    public event Action<ResponseAudioTranscriptDelta>? OnResponseAudioTranscriptDeltaReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseAudioTranscriptDeltaHandler.EventType, out var handler))
            {
                ((ResponseAudioTranscriptDeltaHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseAudioTranscriptDeltaHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseAudioTranscriptDeltaHandler.EventType, out var handler))
            {
                ((ResponseAudioTranscriptDeltaHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseAudioTranscriptDeltaHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response audio transcript done message is processed.
    /// </summary>
    public event Action<ResponseAudioTranscriptDone>? OnResponseAudioTranscriptDoneReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseAudioTranscriptDoneHandler.EventType, out var handler))
            {
                ((ResponseAudioTranscriptDoneHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseAudioTranscriptDoneHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseAudioTranscriptDoneHandler.EventType, out var handler))
            {
                ((ResponseAudioTranscriptDoneHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseAudioTranscriptDoneHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response content part added message is processed.
    /// </summary>
    public event Action<ResponseContentPartAdded>? OnResponseContentPartAddedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseContentPartAddedHandler.EventType, out var handler))
            {
                ((ResponseContentPartAddedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseContentPartAddedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseContentPartAddedHandler.EventType, out var handler))
            {
                ((ResponseContentPartAddedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseContentPartAddedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response content part done message is processed.
    /// </summary>
    public event Action<ResponseContentPartDone>? OnResponseContentPartDoneReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseContentPartDoneHandler.EventType, out var handler))
            {
                ((ResponseContentPartDoneHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseContentPartDoneHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseContentPartDoneHandler.EventType, out var handler))
            {
                ((ResponseContentPartDoneHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseContentPartDoneHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response created message is processed.
    /// </summary>
    public event Action<ResponseCreated>? OnResponseCreatedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseCreatedHandler.EventType, out var handler))
            {
                ((ResponseCreatedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseCreatedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseCreatedHandler.EventType, out var handler))
            {
                ((ResponseCreatedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseCreatedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response done message is processed.
    /// </summary>
    public event Action<ResponseDone>? OnResponseDoneReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseDoneHandler.EventType, out var handler))
            {
                ((ResponseDoneHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseDoneHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseDoneHandler.EventType, out var handler))
            {
                ((ResponseDoneHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseDoneHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response function call arguments delta message is processed.
    /// </summary>
    public event Action<ResponseFunctionCallArgumentsDeltaMessage>? OnResponseFunctionCallArgumentsDeltaReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseFunctionCallArgumentsDeltaHandler.EventType, out var handler))
            {
                ((ResponseFunctionCallArgumentsDeltaHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseFunctionCallArgumentsDeltaHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseFunctionCallArgumentsDeltaHandler.EventType, out var handler))
            {
                ((ResponseFunctionCallArgumentsDeltaHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException(
                    "Handler not registered for ResponseFunctionCallArgumentsDeltaHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response function call arguments done message is processed.
    /// </summary>
    public event Action<ResponseFunctionCallArgumentsDoneMessage>? OnResponseFunctionCallArgumentsDoneReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseFunctionCallArgumentsDoneHandler.EventType, out var handler))
            {
                ((ResponseFunctionCallArgumentsDoneHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseFunctionCallArgumentsDoneHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseFunctionCallArgumentsDoneHandler.EventType, out var handler))
            {
                ((ResponseFunctionCallArgumentsDoneHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException(
                    "Handler not registered for ResponseFunctionCallArgumentsDoneHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response output Item added message is processed.
    /// </summary>
    public event Action<ResponseOutputItemAdded>? OnResponseOutputItemAddedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseOutputItemAddedHandler.EventType, out var handler))
            {
                ((ResponseOutputItemAddedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseOutputItemAddedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseOutputItemAddedHandler.EventType, out var handler))
            {
                ((ResponseOutputItemAddedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseOutputItemAddedHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response output Item done message is processed.
    /// </summary>
    public event Action<ResponseOutputItemDone>? OnResponseOutputItemDoneReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseOutputItemDoneHandler.EventType, out var handler))
            {
                ((ResponseOutputItemDoneHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseOutputItemDoneHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseOutputItemDoneHandler.EventType, out var handler))
            {
                ((ResponseOutputItemDoneHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseOutputItemDoneHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response text delta message is processed.
    /// </summary>
    public event Action<ResponseTextDeltaMessage>? OnResponseTextDeltaReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseTextDeltaHandler.EventType, out var handler))
            {
                ((ResponseTextDeltaHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseTextDeltaHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseTextDeltaHandler.EventType, out var handler))
            {
                ((ResponseTextDeltaHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseTextDeltaHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a response text done message is processed.
    /// </summary>
    public event Action<ResponseTextDoneMessage>? OnResponseTextDoneReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(ResponseTextDoneHandler.EventType, out var handler))
            {
                ((ResponseTextDoneHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new ResponseTextDoneHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(ResponseTextDoneHandler.EventType, out var handler))
            {
                ((ResponseTextDoneHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for ResponseTextDoneHandler.");
            }
        }
    }

    /// <summary>
    ///     Event fired when a session created message is processed.
    /// </summary>
    public event Action<SessionCreated>? OnSessionCreatedReceived
    {
        add
        {
            if (messageHandlers.TryGetValue(SessionCreatedHandler.EventType, out var handler))
            {
                ((SessionCreatedHandler)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new SessionCreatedHandler();
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }
        remove
        {
            if (messageHandlers.TryGetValue(SessionCreatedHandler.EventType, out var handler))
            {
                ((SessionCreatedHandler)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException("Handler not registered for SessionCreatedHandler.");
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    ///     Establishes a WebSocket connection to the VoiceInfo Live API.
    /// </summary>
    /// <returns>A task representing the asynchronous connect operation.</returns>
    public virtual async Task ConnectAsync(ClientSessionUpdate sessionUpdated)
    {
        try
        {
            // Setup authentication
            await SetupAuthenticationAsync();

            var uri = BuildConnectionUri();

            LogMessage($"Connecting to: {uri}");
            LogMessage($"Auth Method: {AuthMethod}");

            await WebSocket.ConnectAsync(new Uri(uri), CancellationTokenSource.Token);

            await SendMessageAsync(sessionUpdated);

            ReceiveTask = ReceiveLoop();
        }
        catch (Exception ex)
        {
            LogError($"Connection failed: {ex.Message}", ex);
            throw;
        }
    }


    /// <summary>
    ///     Disconnects from the VoiceInfo Live API and cleans up resources.
    /// </summary>
    /// <returns>A task representing the asynchronous disconnect operation.</returns>
    public virtual async Task DisconnectAsync()
    {
        try
        {
            CancellationTokenSource.Cancel();

            if (WebSocket.State == WebSocketState.Open)
            {
                await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }

            await ReceiveTask;
        }
        catch (Exception ex)
        {
            LogError($"Disconnection failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    ///     Gets the number of audio chunks available in the output queue.
    /// </summary>
    /// <returns>The number of audio chunks in the queue.</returns>
    public virtual int GetAudioQueueCount()
    {
        return AudioOutputQueue.Count;
    }

    /// <summary>
    ///     Clears all audio data from the output queue.
    /// </summary>
    public virtual void ClearAudioQueue()
    {
        while (AudioOutputQueue.TryDequeue(out _))
        {
        }
    }

    #endregion

    #region Protected Abstract Methods

    /// <summary>
    ///     Builds the WebSocket connection URI specific to the implementation.
    /// </summary>
    /// <returns>The connection URI string.</returns>
    protected abstract string BuildConnectionUri();

    /// <summary>
    ///     Sets up authentication specific to the implementation.
    /// </summary>
    /// <returns>A task representing the asynchronous authentication setup.</returns>
    protected abstract Task SetupAuthenticationAsync();

    /// <summary>
    ///     Registers a message handler for a specific message type.
    /// </summary>
    /// <param name="handler">The message handler to register.</param>
    protected void RegisterMessageHandler(IVoiceLiveHandler handler)
    {
        messageHandlers[handler.MessageType] = handler;
        LogMessage($"Registered handler for message type: {handler.MessageType}");
    }

    #endregion

    #region Protected Methods

    /// <summary>
    ///     Logs a message for debugging or informational purposes.
    /// </summary>
    /// <param name="message">The message to log.</param>
    protected virtual void LogMessage(string message)
    {
        var msg = message.Substring(0, message.Length > 50 ? 50 : message.Length) + "...";
        Logger?.LogInformation(msg);
    }

    /// <summary>
    ///     Sends a message asynchronously.
    /// </summary>
    /// <param name="message">The message object to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task SendMessageAsync(object message)
    {
        var json = JsonSerializer.Serialize(message,new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });
        var buffer = Encoding.UTF8.GetBytes(json);

        await WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            CancellationTokenSource.Token);
    }

    /// <summary>
    /// Sends a message to the server asynchronously.
    /// </summary>
    /// <param name="message">The message object to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal async Task SendServerAsync(object message)
    {
        await SendMessageAsync(message);
    }

    /// <summary>
    ///     Logs an error message along with an exception.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="exception">The exception associated with the error.</param>
    protected virtual void LogError(string message, Exception? exception)
    {
        Logger?.LogError(exception, message);
    }

    #endregion

    #region Private Methods

    private async Task ReceiveLoop()
    {
        var buffer = new byte[1024];
        var messageBuffer = new StringBuilder();

        try
        {
            using (var writer = new StreamWriter(new FileStream("log.txt", FileMode.Append)))
            {
                while (WebSocket.State == WebSocketState.Open && !CancellationTokenSource.Token.IsCancellationRequested)
                {
                    var result =
                        await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageChunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuffer.Append(messageChunk);

                        if (result.EndOfMessage)
                        {
                            var completeMessage = messageBuffer.ToString();
                            messageBuffer.Clear();
                            writer.WriteLine($"[{DateTime.Now:O} : {completeMessage}]");
                            
                            ProcessMessage(completeMessage);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception ex) when (!CancellationTokenSource.Token.IsCancellationRequested)
        {
            LogError($"Receive loop error: {ex.Message}", ex);
        }
    }

    private async void ProcessMessage(string message)
    {
        try
        {
            using var document = JsonDocument.Parse(message);
            var root = document.RootElement;

            if (!root.TryGetProperty("type", out var typeElement))
            {
                LogError("Message has no type field", null);
                return;
            }

            var messageType = typeElement.GetString();
            if (string.IsNullOrEmpty(messageType))
            {
                LogError("Message type is null or empty", null);
                return;
            }

            // Try to handle with registered handlers first
            if (messageHandlers.TryGetValue(messageType, out var handler))
            {
                await handler.HandleAsync(root);
            }
        }
        catch (JsonException ex)
        {
            LogError($"Failed to parse JSON message: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            LogError($"Failed to process message: {ex.Message}", ex);
        }
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    ///     Releases all resources used by the VoiceLiveAPIClientBase.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Releases resources used by the VoiceLiveAPIClientBase.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            CancellationTokenSource.Cancel();
            WebSocket.Dispose();
            CancellationTokenSource.Dispose();
        }
    }

    #endregion
}