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
    ///     Handles error messages.
    /// </summary>
    public class ErrorHandler : VoiceLiveHandlerBase<VoiceLiveError>
    {
        /// <summary>
        ///     Gets the event type for error.
        /// </summary>
        public const string EventType = VoiceLiveError.TypeName;

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } = LoggerFactoryManager.CreateLogger<ErrorHandler>();

        /// <summary>
        ///     Occurs when an error message is processed.
        /// </summary>
        public override event Action<VoiceLiveError> OnProcessMessage;

        /// <summary>
        ///     Handles the error message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<VoiceLiveError>() ??
                       throw new InvalidOperationException("Deserialization failed for VoiceLiveError.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}