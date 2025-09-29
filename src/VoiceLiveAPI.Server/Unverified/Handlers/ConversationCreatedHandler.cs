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
    ///     Handles conversation created messages.
    /// </summary>
    public class ConversationCreatedHandler : VoiceLiveHandlerBase<ConversationCreatedMessage>
    {
        /// <summary>
        ///     Gets the event type for conversation created.
        /// </summary>
        public const string EventType = "conversation.created";

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } = LoggerFactoryManager.CreateLogger<ConversationCreatedHandler>();

        /// <summary>
        ///     Occurs when a conversation created message is processed.
        /// </summary>
        public override event Action<ConversationCreatedMessage> OnProcessMessage;

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