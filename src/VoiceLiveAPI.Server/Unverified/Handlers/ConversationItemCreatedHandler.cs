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

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Handlers
{
    /// <summary>
    ///     Handles conversation Item created messages.
    /// </summary>
    public class ConversationItemCreatedHandler : VoiceLiveHandlerBase<ConversationItemCreated>
    {
        /// <summary>
        ///     Gets the event type for conversation Item created.
        /// </summary>
        public const string EventType = ConversationItemCreated.TypeName;

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the <see cref="ILogger" /> used for logging output.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ConversationItemCreatedHandler>();

        /// <summary>
        ///     Occurs when a conversation Item created message is processed.
        /// </summary>
        public override event Action<ConversationItemCreated> OnProcessMessage;

        /// <summary>
        ///     Handles the conversation Item created message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var conversationItemCreated = message.Deserialize<ConversationItemCreated>() ??
                                          throw new InvalidOperationException(
                                              "Deserialization failed for ConversationItemCreatedMessage.");
            OnProcessMessage?.Invoke(conversationItemCreated);
            await Task.CompletedTask;
        }
    }
}