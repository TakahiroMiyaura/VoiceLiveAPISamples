// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Conversations;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.Conversations
{

    /// <summary>
    ///     Handles conversation created messages.
    /// </summary>
    public class ConversationCreatedHandler : VoiceLiveHandlerBase<ConversationCreatedMessage>
    {
        /// <summary>
        ///     Gets the event type for conversation created.
        /// </summary>
        public static string EventType = "conversation.created";

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Occurs when a conversation created message is processed.
        /// </summary>
        public override event Action<ConversationCreatedMessage> OnProcessMessage = null;

        /// <summary>
        ///     Handles the conversation created message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var messageString = message.GetRawText();
            var conversationCreated = JsonSerializer.Deserialize<ConversationCreatedMessage>(messageString);
            using (var writer = new StreamWriter(new FileStream(EventType + ".txt", FileMode.Create)))
            {
                await writer.WriteAsync(messageString);
                await writer.FlushAsync();
            }

            if (conversationCreated == null)
                throw new InvalidOperationException("Deserialization failed for ConversationCreatedMessage.");
            OnProcessMessage?.Invoke(conversationCreated);
            await Task.CompletedTask;
        }
    }
}