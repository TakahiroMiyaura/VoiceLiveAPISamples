// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Handlers
{
    /// <summary>
    ///     Handles error messages.
    /// </summary>
    [Obsolete("This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers.ErrorHandler instead.")]
    public class ErrorHandler : VoiceLiveHandlerBase<Error>
    {
        /// <summary>
        ///     Gets the event type for error.
        /// </summary>
        public const string EventType = Error.TypeName;

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
        public override event Action<Error> OnProcessMessage;

        /// <summary>
        ///     Handles the error message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<Error>() ??
                       throw new InvalidOperationException("Deserialization failed for ErrorMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}