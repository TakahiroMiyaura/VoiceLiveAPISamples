// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Handlers
{
    /// <summary>
    ///     Handles messages of type ConversationItemInputAudioTranscriptionCompletedMessage.
    /// </summary>
    [Obsolete(
        "This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers.ConversationItemInputAudioTranscriptionCompletedHandler instead.")]
    public class
        ConversationItemInputAudioTranscriptionCompletedHandler :
        VoiceLiveHandlerBase<ConversationItemInputAudioTranscriptionCompleted>, IVoiceLiveHandler
    {
        /// <summary>
        ///     The event type associated with this handler.
        /// </summary>
        public const string EventType = ConversationItemInputAudioTranscriptionCompleted.TypeName;

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Handles the incoming message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to process.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var conversationItemTranscription =
                message.Deserialize<ConversationItemInputAudioTranscriptionCompleted>() ??
                throw new InvalidOperationException(
                    "Deserialization failed for ConversationItemInputAudioTranscriptionCompletedMessage.");
            OnProcessMessage?.Invoke(conversationItemTranscription);
            await Task.CompletedTask;
        }


        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ConversationItemInputAudioTranscriptionCompletedHandler>();

        /// <summary>
        ///     Event triggered when a message of type ConversationItemInputAudioTranscriptionCompletedMessage is processed.
        /// </summary>
        public override event Action<ConversationItemInputAudioTranscriptionCompleted> OnProcessMessage;
    }
}