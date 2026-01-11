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
    ///     Handles response output Item added messages.
    /// </summary>
    [Obsolete(
        "This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers.ResponseOutputItemAddedHandler instead.")]
    public class ResponseOutputItemAddedHandler : VoiceLiveHandlerBase<ResponseOutputItemAdded>
    {
        /// <summary>
        ///     Gets the event type for response output Item added.
        /// </summary>
        public const string EventType = ResponseOutputItemAdded.TypeName;

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ResponseOutputItemAddedHandler>();

        /// <summary>
        ///     Occurs when a response output Item added message is processed.
        /// </summary>
        public override event Action<ResponseOutputItemAdded> OnProcessMessage;

        /// <summary>
        ///     Handles the response output Item added message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ResponseOutputItemAdded>() ??
                       throw new InvalidOperationException(
                           "Deserialization failed for ResponseOutputItemAddedMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}