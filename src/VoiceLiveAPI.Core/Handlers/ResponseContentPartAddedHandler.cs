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
    ///     Handles response content part added messages.
    /// </summary>
    public class ResponseContentPartAddedHandler : VoiceLiveHandlerBase<ContentPartAdded>
    {
        /// <summary>
        ///     Gets the event type for response content part added.
        /// </summary>
        public const string EventType = ContentPartAdded.TypeName;

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ResponseContentPartAddedHandler>();

        /// <summary>
        ///     Occurs when a response content part added message is processed.
        /// </summary>
        public override event Action<ContentPartAdded> OnProcessMessage;

        /// <summary>
        ///     Handles the response content part added message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ContentPartAdded>() ??
                       throw new InvalidOperationException(
                           "Deserialization failed for ContentPartAdded.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}