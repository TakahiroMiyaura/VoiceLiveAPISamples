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
    ///     Handles response done messages.
    /// </summary>
    public class ResponseDoneHandler : VoiceLiveHandlerBase<ResponseInfo>
    {
        /// <summary>
        ///     Gets the event type for response done.
        /// </summary>
        public const string EventType = ResponseInfo.TypeName;

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } = LoggerFactoryManager.CreateLogger<ResponseDoneHandler>();

        /// <summary>
        ///     Occurs when a response done message is processed.
        /// </summary>
        public override event Action<ResponseInfo> OnProcessMessage;

        /// <summary>
        ///     Handles the response done message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ResponseInfo>() ??
                       throw new InvalidOperationException("Deserialization failed for ResponseInfo.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}