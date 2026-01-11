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
    ///     Handles conversation Item deleted messages.
    /// </summary>
    public class ConversationItemDeletedHandler : VoiceLiveHandlerBase<ItemDeleted>
    {
        /// <summary>
        ///     Gets the event type for conversation Item deleted.
        /// </summary>
        public const string EventType = ItemDeleted.TypeName;

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
        public override event Action<ItemDeleted> OnProcessMessage;

        /// <summary>
        ///     Handles the conversation Item deleted message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var conversationItemDeleted = message.Deserialize<ItemDeleted>() ??
                                          throw new InvalidOperationException(
                                              "Deserialization failed for ItemDeleted.");
            OnProcessMessage?.Invoke(conversationItemDeleted);
            await Task.CompletedTask;
        }
    }
}