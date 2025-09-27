// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses
{

    /// <summary>
    ///     Handles response done messages.
    /// </summary>
    public class ResponseDoneHandler : VoiceLiveHandlerBase<ResponseDone>
    {
        #region Static Fields and Constants

        /// <summary>
        ///     Gets the event type for response done.
        /// </summary>
        public static string EventType = ResponseDone.Type;

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a response done message is processed.
        /// </summary>
        public override event Action<ResponseDone> OnProcessMessage = null;

        #endregion

        #region Properties, Indexers

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        #endregion

        #region Public methods

        /// <summary>
        ///     Handles the response done message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails for the response done message.</exception>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ResponseDone>();
            if (json == null)
                throw new InvalidOperationException("Deserialization failed for ResponseDoneMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }

        #endregion
    }
}