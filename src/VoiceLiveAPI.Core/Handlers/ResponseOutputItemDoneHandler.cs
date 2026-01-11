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
    ///     Handles response output Item done messages.
    /// </summary>
    public class ResponseOutputItemDoneHandler : VoiceLiveHandlerBase<OutputItemDone>
    {
        /// <summary>
        ///     Gets the event type for response output Item done.
        /// </summary>
        public const string EventType = OutputItemDone.TypeName;

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ResponseOutputItemDoneHandler>();

        /// <summary>
        ///     Occurs when a response output Item done message is processed.
        /// </summary>
        public override event Action<OutputItemDone> OnProcessMessage;

        /// <summary>
        ///     Handles the response output Item done message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<OutputItemDone>() ??
                       throw new InvalidOperationException("Deserialization failed for OutputItemDone.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}