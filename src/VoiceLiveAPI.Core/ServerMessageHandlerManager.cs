// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers.Unverified;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Unverified;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core
{
    /// <summary>
    ///     Manages server-side message handlers for VoiceLiveAPI.
    ///     Registers and dispatches events for various message types received from the server.
    /// </summary>
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
        public event Action<VisemeDelta> OnResponseAnimationVisemeDeltaReceived
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
                    throw new InvalidOperationException("Handler not registered for ResponseAudioDeltaHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when an response animation viseme done is received.
        /// </summary>
        public event Action<VisemeDone> OnResponseAnimationVisemeDoneReceived
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
                    throw new InvalidOperationException("Handler not registered for ResponseAudioDeltaHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when an audio delta response is received.
        /// </summary>
        public event Action<AudioDelta> OnAudioDeltaReceived
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
        ///     Event fired when a video delta response is received (WebSocket avatar video frames,
        ///     <c>response.video.delta</c>). Available when the avatar uses <c>output_protocol=websocket</c>.
        /// </summary>
        public event Action<VideoDelta> OnVideoDeltaReceived
        {
            add
            {
                if (TryGetValue(ResponseVideoDeltaHandler.EventType, out var handler))
                {
                    ((ResponseVideoDeltaHandler)handler).OnProcessMessage += value;
                }
                else
                {
                    var h = new ResponseVideoDeltaHandler();
                    h.OnProcessMessage += value;
                    RegisterMessageHandler(h);
                }
            }
            remove
            {
                if (TryGetValue(ResponseVideoDeltaHandler.EventType, out var handler))
                {
                    ((ResponseVideoDeltaHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException("Handler not registered for ResponseVideoDeltaHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when a WebRTC voice-session SDP answer is received (<c>rtc.call.sdp.created</c>).
        ///     Apply the answer as the peer's remote description to complete negotiation.
        /// </summary>
        public event Action<RtcCallSdpCreated> OnRtcCallSdpCreatedReceived
        {
            add
            {
                if (TryGetValue(RtcCallSdpCreatedHandler.EventType, out var handler))
                {
                    ((RtcCallSdpCreatedHandler)handler).OnProcessMessage += value;
                }
                else
                {
                    var h = new RtcCallSdpCreatedHandler();
                    h.OnProcessMessage += value;
                    RegisterMessageHandler(h);
                }
            }
            remove
            {
                if (TryGetValue(RtcCallSdpCreatedHandler.EventType, out var handler))
                {
                    ((RtcCallSdpCreatedHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException("Handler not registered for RtcCallSdpCreatedHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when a WebRTC voice-session operation error is received (<c>rtc.call.error</c>).
        /// </summary>
        public event Action<RtcCallError> OnRtcCallErrorReceived
        {
            add
            {
                if (TryGetValue(RtcCallErrorHandler.EventType, out var handler))
                {
                    ((RtcCallErrorHandler)handler).OnProcessMessage += value;
                }
                else
                {
                    var h = new RtcCallErrorHandler();
                    h.OnProcessMessage += value;
                    RegisterMessageHandler(h);
                }
            }
            remove
            {
                if (TryGetValue(RtcCallErrorHandler.EventType, out var handler))
                {
                    ((RtcCallErrorHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException("Handler not registered for RtcCallErrorHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when a transcription is received.
        /// </summary>
        public event Action<TranscriptionResult> OnTranscriptionReceived
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
                    throw new InvalidOperationException("Handler not registered for ResponseAudioDeltaHandler.");
                }
            }
        }

        /// <summary>
        ///     Event fired when a Session update response is received.
        /// </summary>
        public event Action<SessionInfo> OnSessionUpdateReceived
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
                    throw new InvalidOperationException("Handler not registered for ResponseAudioDeltaHandler.");
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
        public event Action<ItemCreated> OnConversationItemCreatedReceived
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
        public event Action<ItemDeleted> OnConversationItemDeletedReceived
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
        public event Action<VoiceLiveError> OnErrorReceived
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
        public event Action<AudioCommitted> OnInputAudioBufferCommittedReceived
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
        public event Action<SpeechStarted> OnInputAudioBufferSpeechStartedReceived
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
        public event Action<SpeechStopped> OnInputAudioBufferSpeechStoppedReceived
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
        public event Action<AudioDone> OnResponseAudioDoneReceived
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
        public event Action<TranscriptDelta> OnResponseAudioTranscriptDeltaReceived
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
        public event Action<TranscriptDone> OnResponseAudioTranscriptDoneReceived
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
        public event Action<ContentPartAdded> OnResponseContentPartAddedReceived
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
        public event Action<ContentPartDone> OnResponseContentPartDoneReceived
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
        public event Action<ResponseInfo> OnResponseDoneReceived
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
        public event Action<OutputItemAdded> OnResponseOutputItemAddedReceived
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
        public event Action<OutputItemDone> OnResponseOutputItemDoneReceived
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
        ///     Event fired when a function call arguments delta is received.
        /// </summary>
        public event Action<FunctionCallDelta> OnFunctionCallDeltaReceived
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
        ///     Event fired when a function call is completed with all arguments.
        /// </summary>
        public event Action<FunctionCallDone> OnFunctionCallDoneReceived
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
        public event Action<SessionInfo> OnSessionCreatedReceived
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

        /// <summary>
        ///     Event fired when MCP list tools starts.
        /// </summary>
        public event Action<McpListToolsInProgress> OnMcpListToolsInProgressReceived
        {
            add => SubscribeGenericHandler(McpListToolsInProgress.TypeName, value);
            remove => UnsubscribeGenericHandler<McpListToolsInProgress>(McpListToolsInProgress.TypeName, value);
        }

        /// <summary>
        ///     Event fired when MCP list tools completes.
        /// </summary>
        public event Action<McpListToolsCompleted> OnMcpListToolsCompletedReceived
        {
            add => SubscribeGenericHandler(McpListToolsCompleted.TypeName, value);
            remove => UnsubscribeGenericHandler<McpListToolsCompleted>(McpListToolsCompleted.TypeName, value);
        }

        /// <summary>
        ///     Event fired when MCP list tools fails.
        /// </summary>
        public event Action<McpListToolsFailed> OnMcpListToolsFailedReceived
        {
            add => SubscribeGenericHandler(McpListToolsFailed.TypeName, value);
            remove => UnsubscribeGenericHandler<McpListToolsFailed>(McpListToolsFailed.TypeName, value);
        }

        /// <summary>
        ///     Event fired when MCP call arguments delta is received.
        /// </summary>
        public event Action<McpCallArgumentsDelta> OnMcpCallArgumentsDeltaReceived
        {
            add => SubscribeGenericHandler(McpCallArgumentsDelta.TypeName, value);
            remove => UnsubscribeGenericHandler<McpCallArgumentsDelta>(McpCallArgumentsDelta.TypeName, value);
        }

        /// <summary>
        ///     Event fired when MCP call arguments are complete.
        /// </summary>
        public event Action<McpCallArgumentsDone> OnMcpCallArgumentsDoneReceived
        {
            add => SubscribeGenericHandler(McpCallArgumentsDone.TypeName, value);
            remove => UnsubscribeGenericHandler<McpCallArgumentsDone>(McpCallArgumentsDone.TypeName, value);
        }

        /// <summary>
        ///     Event fired when an MCP call starts processing.
        /// </summary>
        public event Action<McpCallInProgress> OnMcpCallInProgressReceived
        {
            add => SubscribeGenericHandler(McpCallInProgress.TypeName, value);
            remove => UnsubscribeGenericHandler<McpCallInProgress>(McpCallInProgress.TypeName, value);
        }

        /// <summary>
        ///     Event fired when an MCP call completes successfully.
        /// </summary>
        public event Action<McpCallCompleted> OnMcpCallCompletedReceived
        {
            add => SubscribeGenericHandler(McpCallCompleted.TypeName, value);
            remove => UnsubscribeGenericHandler<McpCallCompleted>(McpCallCompleted.TypeName, value);
        }

        /// <summary>
        ///     Event fired when an MCP call fails.
        /// </summary>
        public event Action<McpCallFailed> OnMcpCallFailedReceived
        {
            add => SubscribeGenericHandler(McpCallFailed.TypeName, value);
            remove => UnsubscribeGenericHandler<McpCallFailed>(McpCallFailed.TypeName, value);
        }

        /// <summary>
        ///     Event fired when hosted Foundry agent tool-call arguments are streamed.
        /// </summary>
        public event Action<FoundryAgentCallArgumentsDelta> OnFoundryAgentCallArgumentsDeltaReceived
        {
            add => SubscribeGenericHandler(FoundryAgentCallArgumentsDelta.TypeName, value);
            remove => UnsubscribeGenericHandler<FoundryAgentCallArgumentsDelta>(FoundryAgentCallArgumentsDelta.TypeName, value);
        }

        /// <summary>
        ///     Event fired when hosted Foundry agent tool-call arguments are complete.
        /// </summary>
        public event Action<FoundryAgentCallArgumentsDone> OnFoundryAgentCallArgumentsDoneReceived
        {
            add => SubscribeGenericHandler(FoundryAgentCallArgumentsDone.TypeName, value);
            remove => UnsubscribeGenericHandler<FoundryAgentCallArgumentsDone>(FoundryAgentCallArgumentsDone.TypeName, value);
        }

        /// <summary>
        ///     Event fired when a hosted Foundry agent call starts processing.
        /// </summary>
        public event Action<FoundryAgentCallInProgress> OnFoundryAgentCallInProgressReceived
        {
            add => SubscribeGenericHandler(FoundryAgentCallInProgress.TypeName, value);
            remove => UnsubscribeGenericHandler<FoundryAgentCallInProgress>(FoundryAgentCallInProgress.TypeName, value);
        }

        /// <summary>
        ///     Event fired when a hosted Foundry agent call completes successfully.
        /// </summary>
        public event Action<FoundryAgentCallCompleted> OnFoundryAgentCallCompletedReceived
        {
            add => SubscribeGenericHandler(FoundryAgentCallCompleted.TypeName, value);
            remove => UnsubscribeGenericHandler<FoundryAgentCallCompleted>(FoundryAgentCallCompleted.TypeName, value);
        }

        /// <summary>
        ///     Event fired when a hosted Foundry agent call fails.
        /// </summary>
        public event Action<FoundryAgentCallFailed> OnFoundryAgentCallFailedReceived
        {
            add => SubscribeGenericHandler(FoundryAgentCallFailed.TypeName, value);
            remove => UnsubscribeGenericHandler<FoundryAgentCallFailed>(FoundryAgentCallFailed.TypeName, value);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Subscribes a handler using <see cref="GenericServerEventHandler{T}" />.
        /// </summary>
        /// <typeparam name="T">The server event model type.</typeparam>
        /// <param name="eventType">The event type string.</param>
        /// <param name="value">The event handler delegate.</param>
        private void SubscribeGenericHandler<T>(string eventType, Action<T> value)
            where T : Events.ServerEvent
        {
            if (TryGetValue(eventType, out var handler))
            {
                ((GenericServerEventHandler<T>)handler).OnProcessMessage += value;
            }
            else
            {
                var h = new GenericServerEventHandler<T>(eventType);
                h.OnProcessMessage += value;
                RegisterMessageHandler(h);
            }
        }

        /// <summary>
        ///     Unsubscribes a handler using <see cref="GenericServerEventHandler{T}" />.
        /// </summary>
        /// <typeparam name="T">The server event model type.</typeparam>
        /// <param name="eventType">The event type string.</param>
        /// <param name="value">The event handler delegate.</param>
        private void UnsubscribeGenericHandler<T>(string eventType, Action<T> value)
            where T : Events.ServerEvent
        {
            if (TryGetValue(eventType, out var handler))
            {
                ((GenericServerEventHandler<T>)handler).OnProcessMessage -= value;
            }
            else
            {
                throw new InvalidOperationException($"Handler not registered for {eventType}.");
            }
        }

        #endregion
    }
}