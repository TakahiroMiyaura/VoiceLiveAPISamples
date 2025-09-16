// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Conversations.Items;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.Conversations.Items
{

    /// <summary>
    ///     Handles conversation Item created messages.
    /// </summary>
    public class ConversationItemCreatedHandler : VoiceLiveHandlerBase<ConversationItemCreated>
    {
        /// <summary>
        ///     Gets the event type for conversation Item created.
        /// </summary>
        public static string EventType = ConversationItemCreated.Type;

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Occurs when a conversation Item created message is processed.
        /// </summary>
        public override event Action<ConversationItemCreated> OnProcessMessage = null;

        /// <summary>
        ///     Handles the conversation Item created message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var conversationItemCreated = message.Deserialize<ConversationItemCreated>();
            if (conversationItemCreated == null)
                throw new InvalidOperationException("Deserialization failed for ConversationItemCreatedMessage.");
            OnProcessMessage?.Invoke(conversationItemCreated);
            await Task.CompletedTask;
        }
    }
}