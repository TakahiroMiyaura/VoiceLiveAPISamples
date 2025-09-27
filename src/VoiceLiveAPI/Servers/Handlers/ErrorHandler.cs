// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers
{

    /// <summary>
    ///     Handles error messages.
    /// </summary>
    public class ErrorHandler : VoiceLiveHandlerBase<Error>
    {
        #region Static Fields and Constants

        /// <summary>
        ///     Gets the event type for error.
        /// </summary>
        public static string EventType = Error.Type;

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when an error message is processed.
        /// </summary>
        public override event Action<Error> OnProcessMessage = null;

        #endregion

        #region Properties, Indexers

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        #endregion

        #region Public methods

        /// <summary>
        ///     Handles the error message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails for the error message.</exception>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<Error>();
            if (json == null)
                throw new InvalidOperationException("Deserialization failed for ErrorMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }

        #endregion
    }
}