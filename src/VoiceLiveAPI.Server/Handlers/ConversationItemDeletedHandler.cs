// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Messages;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Handlers
{
    /// <summary>
    ///     Handles conversation Item deleted messages.
    /// </summary>
    public class ConversationItemDeletedHandler : VoiceLiveHandlerBase<ConversationItemDeletedMessage>
    {
        /// <summary>
        ///     Gets the event type for conversation Item deleted.
        /// </summary>
        public const string EventType = "conversation.Item.deleted";

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ConversationItemDeletedHandler>();

        /// <summary>
        ///     Occurs when a conversation Item deleted message is processed.
        /// </summary>
        public override event Action<ConversationItemDeletedMessage> OnProcessMessage;

        /// <summary>
        ///     Handles the conversation Item deleted message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var conversationItemDeleted = message.Deserialize<ConversationItemDeletedMessage>() ??
                                          throw new InvalidOperationException(
                                              "Deserialization failed for ConversationItemDeletedMessage.");
            OnProcessMessage?.Invoke(conversationItemDeleted);
            await Task.CompletedTask;
        }
    }
}