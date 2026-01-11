// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Handlers;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Handlers;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Messages;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server
{
    /// <summary>
    ///     Manages server-side message handlers for VoiceLiveAPI.
    ///     Registers and dispatches events for various message types received from the server.
    /// </summary>
    [Obsolete("This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.ServerMessageHandlerManager instead.")]
    public class ServerMessageHandlerManager : MessageHandlerManagerBase
    {
        /// <summary>
        ///     Gets or sets the <see cref="ILogger" /> instance used for logging output.
        /// </summary>
        public override ILogger Logger { set; get; } = LoggerFactoryManager.CreateLogger<ServerMessageHandlerManager>();

        #region Events

        /// <summary>
        ///     Event fired when a response animation viseme delta is received.
        /// </summary>
        public event Action<ResponseAnimationVisemeDelta> OnResponseAnimationVisemeDeltaReceived
        {
            add
            {
                if (TryGetValue(ResponseAnimationVisemeDeltaHandler.EventType, out var handler))
                {
                    ((ResponseAnimationVisemeDeltaHandler)handler).OnProcessMessage += value;
                }
                else
                {
                    var h = new ResponseAnimationVisemeDeltaHandler();
                    h.OnProcessMessage += value;
                    RegisterMessageHandler(h);
                }
            }
            remove
            {
                if (TryGetValue(ResponseAnimationVisemeDeltaHandler.EventType, out var handler))
                {
                    ((ResponseAnimationVisemeDeltaHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException("Handler not registered for ResponseAnimationVisemeDeltaHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when an response animation viseme done is received.
        /// </summary>
        public event Action<ResponseAnimationVisemeDone> OnResponseAnimationVisemeDoneReceived
        {
            add
            {
                if (TryGetValue(ResponseAnimationVisemeDoneHandler.EventType, out var handler))
                {
                    ((ResponseAnimationVisemeDoneHandler)handler).OnProcessMessage += value;
                }
                else
                {
                    var h = new ResponseAnimationVisemeDoneHandler();
                    h.OnProcessMessage += value;
                    RegisterMessageHandler(h);
                }
            }
            remove
            {
                if (TryGetValue(ResponseAnimationVisemeDoneHandler.EventType, out var handler))
                {
                    ((ResponseAnimationVisemeDoneHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException("Handler not registered for ResponseAnimationVisemeDoneHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when an audio delta response is received.
        /// </summary>
        public event Action<ResponseAudioDelta> OnAudioDeltaReceived
        {
            add
            {
                if (TryGetValue(ResponseAudioDeltaHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseAudioDeltaHandler.EventType, out var handler))
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
        public event Action<ConversationItemInputAudioTranscriptionCompleted> OnTranscriptionReceived
        {
            add
            {
                if (TryGetValue(ConversationItemInputAudioTranscriptionCompletedHandler.EventType,
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
                if (TryGetValue(ConversationItemInputAudioTranscriptionCompletedHandler.EventType,
                        out var handler))
                {
                    ((ConversationItemInputAudioTranscriptionCompletedHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException("Handler not registered for ConversationItemInputAudioTranscriptionCompletedHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when a Session update response is received.
        /// </summary>
        public event Action<ServerSessionUpdated> OnSessionUpdateReceived
        {
            add
            {
                if (TryGetValue(ServerSessionUpdateHandler.EventType, out var handler))
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
                if (TryGetValue(ServerSessionUpdateHandler.EventType, out var handler))
                {
                    ((ServerSessionUpdateHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException("Handler not registered for ServerSessionUpdateHandler.");
                }
            }
        }

        // Server Events
        /// <summary>
        ///     Event fired when a conversation created message is processed.
        /// </summary>
        public event Action<ConversationCreatedMessage> OnConversationCreatedReceived
        {
            add
            {
                if (TryGetValue(ConversationCreatedHandler.EventType, out var handler))
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
                if (TryGetValue(ConversationCreatedHandler.EventType, out var handler))
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
        public event Action<ConversationItemCreated> OnConversationItemCreatedReceived
        {
            add
            {
                if (TryGetValue(ConversationItemCreatedHandler.EventType, out var handler))
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
                if (TryGetValue(ConversationItemCreatedHandler.EventType, out var handler))
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
        public event Action<ConversationItemRetrievedMessage> OnConversationItemRetrievedReceived
        {
            add
            {
                if (TryGetValue(ConversationItemRetrievedHandler.EventType, out var handler))
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
                if (TryGetValue(ConversationItemRetrievedHandler.EventType, out var handler))
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
        public event Action<ConversationItemDeletedMessage> OnConversationItemDeletedReceived
        {
            add
            {
                if (TryGetValue(ConversationItemDeletedHandler.EventType, out var handler))
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
                if (TryGetValue(ConversationItemDeletedHandler.EventType, out var handler))
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
        public event Action<ConversationItemInputAudioTranscriptionFailedMessage>
            OnConversationItemInputAudioTranscriptionFailedReceived
            {
                add
                {
                    if (TryGetValue(ConversationItemInputAudioTranscriptionFailedHandler.EventType,
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
                    if (TryGetValue(ConversationItemInputAudioTranscriptionFailedHandler.EventType,
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
        public event Action<ConversationItemTruncatedMessage> OnConversationItemTruncatedReceived
        {
            add
            {
                if (TryGetValue(ConversationItemTruncatedHandler.EventType, out var handler))
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
                if (TryGetValue(ConversationItemTruncatedHandler.EventType, out var handler))
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
        public event Action<Error> OnErrorReceived
        {
            add
            {
                if (TryGetValue(ErrorHandler.EventType, out var handler))
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
                if (TryGetValue(ErrorHandler.EventType, out var handler))
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
        ///     Event fired when an input audio buffer committed message is processed.
        /// </summary>
        public event Action<InputAudioBufferCommitted> OnInputAudioBufferCommittedReceived
        {
            add
            {
                if (TryGetValue(InputAudioBufferCommittedHandler.EventType, out var handler))
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
                if (TryGetValue(InputAudioBufferCommittedHandler.EventType, out var handler))
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
        public event Action<InputAudioBufferSpeechStarted> OnInputAudioBufferSpeechStartedReceived
        {
            add
            {
                if (TryGetValue(InputAudioBufferSpeechStartedHandler.EventType, out var handler))
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
                if (TryGetValue(InputAudioBufferSpeechStartedHandler.EventType, out var handler))
                {
                    ((InputAudioBufferSpeechStartedHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Handler not registered for InputAudioBufferSpeechStartedHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when an input audio buffer speech stopped message is processed.
        /// </summary>
        public event Action<InputAudioBufferSpeechStopped> OnInputAudioBufferSpeechStoppedReceived
        {
            add
            {
                if (TryGetValue(InputAudioBufferSpeechStoppedHandler.EventType, out var handler))
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
                if (TryGetValue(InputAudioBufferSpeechStoppedHandler.EventType, out var handler))
                {
                    ((InputAudioBufferSpeechStoppedHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Handler not registered for InputAudioBufferSpeechStoppedHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when a response audio done message is processed.
        /// </summary>
        public event Action<ResponseAudioDone> OnResponseAudioDoneReceived
        {
            add
            {
                if (TryGetValue(ResponseAudioDoneHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseAudioDoneHandler.EventType, out var handler))
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
        public event Action<ResponseAudioTranscriptDelta> OnResponseAudioTranscriptDeltaReceived
        {
            add
            {
                if (TryGetValue(ResponseAudioTranscriptDeltaHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseAudioTranscriptDeltaHandler.EventType, out var handler))
                {
                    ((ResponseAudioTranscriptDeltaHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Handler not registered for ResponseAudioTranscriptDeltaHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when a response audio transcript done message is processed.
        /// </summary>
        public event Action<ResponseAudioTranscriptDone> OnResponseAudioTranscriptDoneReceived
        {
            add
            {
                if (TryGetValue(ResponseAudioTranscriptDoneHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseAudioTranscriptDoneHandler.EventType, out var handler))
                {
                    ((ResponseAudioTranscriptDoneHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Handler not registered for ResponseAudioTranscriptDoneHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when a response content part added message is processed.
        /// </summary>
        public event Action<ResponseContentPartAdded> OnResponseContentPartAddedReceived
        {
            add
            {
                if (TryGetValue(ResponseContentPartAddedHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseContentPartAddedHandler.EventType, out var handler))
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
        public event Action<ResponseContentPartDone> OnResponseContentPartDoneReceived
        {
            add
            {
                if (TryGetValue(ResponseContentPartDoneHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseContentPartDoneHandler.EventType, out var handler))
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
        public event Action<ResponseCreated> OnResponseCreatedReceived
        {
            add
            {
                if (TryGetValue(ResponseCreatedHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseCreatedHandler.EventType, out var handler))
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
        public event Action<ResponseDone> OnResponseDoneReceived
        {
            add
            {
                if (TryGetValue(ResponseDoneHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseDoneHandler.EventType, out var handler))
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
        ///     Event fired when a response output Item added message is processed.
        /// </summary>
        public event Action<ResponseOutputItemAdded> OnResponseOutputItemAddedReceived
        {
            add
            {
                if (TryGetValue(ResponseOutputItemAddedHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseOutputItemAddedHandler.EventType, out var handler))
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
        public event Action<ResponseOutputItemDone> OnResponseOutputItemDoneReceived
        {
            add
            {
                if (TryGetValue(ResponseOutputItemDoneHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseOutputItemDoneHandler.EventType, out var handler))
                {
                    ((ResponseOutputItemDoneHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException("Handler not registered for ResponseOutputItemDoneHandler.");
                }
            }
        }

        #endregion

        #region unconfirmed events

        /// <summary>
        ///     Event fired when an input audio buffer cleared message is processed.
        /// </summary>
        public event Action<InputAudioBufferClearedMessage> OnInputAudioBufferClearedReceived
        {
            add
            {
                if (TryGetValue(InputAudioBufferClearedHandler.EventType, out var handler))
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
                if (TryGetValue(InputAudioBufferClearedHandler.EventType, out var handler))
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
        ///     Event fired when an output audio buffer cleared message is processed.
        /// </summary>
        public event Action<OutputAudioBufferClearedMessage> OnOutputAudioBufferClearedReceived
        {
            add
            {
                if (TryGetValue(OutputAudioBufferClearedHandler.EventType, out var handler))
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
                if (TryGetValue(OutputAudioBufferClearedHandler.EventType, out var handler))
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
        public event Action<OutputAudioBufferStartedMessage> OnOutputAudioBufferStartedReceived
        {
            add
            {
                if (TryGetValue(OutputAudioBufferStartedHandler.EventType, out var handler))
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
                if (TryGetValue(OutputAudioBufferStartedHandler.EventType, out var handler))
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
        public event Action<OutputAudioBufferStoppedMessage> OnOutputAudioBufferStoppedReceived
        {
            add
            {
                if (TryGetValue(OutputAudioBufferStoppedHandler.EventType, out var handler))
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
                if (TryGetValue(OutputAudioBufferStoppedHandler.EventType, out var handler))
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
        public event Action<RateLimitsUpdatedMessage> OnRateLimitsUpdatedReceived
        {
            add
            {
                if (TryGetValue(RateLimitsUpdatedHandler.EventType, out var handler))
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
                if (TryGetValue(RateLimitsUpdatedHandler.EventType, out var handler))
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
        ///     Event fired when a response function call arguments delta message is processed.
        /// </summary>
        public event Action<ResponseFunctionCallArgumentsDeltaMessage> OnResponseFunctionCallArgumentsDeltaReceived
        {
            add
            {
                if (TryGetValue(ResponseFunctionCallArgumentsDeltaHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseFunctionCallArgumentsDeltaHandler.EventType, out var handler))
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
        public event Action<ResponseFunctionCallArgumentsDoneMessage> OnResponseFunctionCallArgumentsDoneReceived
        {
            add
            {
                if (TryGetValue(ResponseFunctionCallArgumentsDoneHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseFunctionCallArgumentsDoneHandler.EventType, out var handler))
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
        ///     Event fired when a response text delta message is processed.
        /// </summary>
        public event Action<ResponseTextDeltaMessage> OnResponseTextDeltaReceived
        {
            add
            {
                if (TryGetValue(ResponseTextDeltaHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseTextDeltaHandler.EventType, out var handler))
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
        public event Action<ResponseTextDoneMessage> OnResponseTextDoneReceived
        {
            add
            {
                if (TryGetValue(ResponseTextDoneHandler.EventType, out var handler))
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
                if (TryGetValue(ResponseTextDoneHandler.EventType, out var handler))
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
        ///     Event fired when a Session created message is processed.
        /// </summary>
        public event Action<SessionCreated> OnSessionCreatedReceived
        {
            add
            {
                if (TryGetValue(SessionCreatedHandler.EventType, out var handler))
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
                if (TryGetValue(SessionCreatedHandler.EventType, out var handler))
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
    }
}