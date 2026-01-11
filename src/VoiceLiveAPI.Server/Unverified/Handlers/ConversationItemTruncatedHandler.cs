// Copyright (c) 2026 Takahiro Miyaura
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
    ///     Handles conversation Item truncated messages.
    /// </summary>
    public class ConversationItemTruncatedHandler : VoiceLiveHandlerBase<ConversationItemTruncatedMessage>
    {
        /// <summary>
        ///     Gets the event type for conversation Item truncated.
        /// </summary>
        public const string EventType = "conversation.Item.truncated";

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ConversationItemTruncatedHandler>();

        /// <summary>
        ///     Occurs when a conversation Item truncated message is processed.
        /// </summary>
        public override event Action<ConversationItemTruncatedMessage> OnProcessMessage;

        /// <summary>
        ///     Handles the conversation Item truncated message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var messageString = message.GetRawText();
            var conversationItemTruncated = JsonSerializer.Deserialize<ConversationItemTruncatedMessage>(messageString);
            using (var writer =
                   new StreamWriter(new FileStream(EventType + ".txt", FileMode.Create)))
            {
                await writer.WriteAsync(messageString);
                await writer.FlushAsync();
            }

            if (conversationItemTruncated == null)
                throw new InvalidOperationException("Deserialization failed for ConversationItemTruncatedMessage.");
            OnProcessMessage?.Invoke(conversationItemTruncated);
            await Task.CompletedTask;
        }
    }
}