// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers
{
    /// <summary>
    ///     Handles messages of type TranscriptionResult.
    /// </summary>
    public class
        ConversationItemInputAudioTranscriptionCompletedHandler :
        VoiceLiveHandlerBase<TranscriptionResult>, IVoiceLiveHandler
    {
        /// <summary>
        ///     The event type associated with this handler.
        /// </summary>
        public const string EventType = TranscriptionResult.TypeName;

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
                message.Deserialize<TranscriptionResult>() ??
                throw new InvalidOperationException(
                    "Deserialization failed for TranscriptionResult.");
            OnProcessMessage?.Invoke(conversationItemTranscription);
            await Task.CompletedTask;
        }


        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ConversationItemInputAudioTranscriptionCompletedHandler>();

        /// <summary>
        ///     Event triggered when a message of type TranscriptionResult is processed.
        /// </summary>
        public override event Action<TranscriptionResult> OnProcessMessage;
    }
}