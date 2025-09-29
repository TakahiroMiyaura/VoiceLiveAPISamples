// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Messages;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Handlers
{
    /// <summary>
    ///     Handles conversation Item input audio transcription failed messages.
    /// </summary>
    public class
        ConversationItemInputAudioTranscriptionFailedHandler : VoiceLiveHandlerBase<
        ConversationItemInputAudioTranscriptionFailedMessage>
    {
        /// <summary>
        ///     Gets the event type for conversation Item input audio transcription failed.
        /// </summary>
        public const string EventType = "conversation.Item.input_audio_transcription.failed";

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the <see cref="ILogger" /> used for logging output in this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ConversationItemInputAudioTranscriptionFailedHandler>();

        /// <summary>
        ///     Occurs when a conversation Item input audio transcription failed message is processed.
        /// </summary>
        public override event Action<ConversationItemInputAudioTranscriptionFailedMessage> OnProcessMessage;

        /// <summary>
        ///     Handles the conversation Item input audio transcription failed message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var messageString = message.GetRawText();
            var conversationItemInputAudioTranscriptionFailed =
                JsonSerializer.Deserialize<ConversationItemInputAudioTranscriptionFailedMessage>(messageString);
            using (var writer =
                   new StreamWriter(new FileStream(EventType + ".txt",
                       FileMode.Create)))
            {
                await writer.WriteAsync(messageString);
                await writer.FlushAsync();
            }

            if (conversationItemInputAudioTranscriptionFailed == null)
                throw new InvalidOperationException(
                    "Deserialization failed for ConversationItemInputAudioTranscriptionFailedMessage.");
            OnProcessMessage?.Invoke(conversationItemInputAudioTranscriptionFailed);
            await Task.CompletedTask;
        }
    }
}