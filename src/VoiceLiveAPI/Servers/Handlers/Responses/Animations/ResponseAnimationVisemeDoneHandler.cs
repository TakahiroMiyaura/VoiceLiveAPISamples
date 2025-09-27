// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.Animations;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses.Animations
{

    /// <summary>
    ///     Handles messages of type "response.animation_viseme.done".
    /// </summary>
    public class ResponseAnimationVisemeDoneHandler : VoiceLiveHandlerBase<ResponseAnimationVisemeDone>
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The event type associated with this handler.
        /// </summary>
        public static string EventType = ResponseAnimationVisemeDone.Type;

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a message of type <see cref="ResponseAnimationVisemeDone" /> is processed.
        /// </summary>
        public override event Action<ResponseAnimationVisemeDone> OnProcessMessage = null;

        #endregion

        #region Properties, Indexers

        /// <summary>
        ///     Gets the message type handled by this handler.
        /// </summary>
        public override string MessageType => EventType;

        #endregion

        #region Public methods

        /// <summary>
        ///     Handles the incoming message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ResponseAnimationVisemeDone>();
            if (json == null)
                throw new InvalidOperationException("Deserialization failed for ResponseAnimationVisemeDone.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }

        #endregion
    }
}