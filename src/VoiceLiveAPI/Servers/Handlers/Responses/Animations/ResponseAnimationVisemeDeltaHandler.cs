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
    ///     Handles messages of type "response.animation_viseme.delta".
    /// </summary>
    public class ResponseAnimationVisemeDeltaHandler : VoiceLiveHandlerBase<ResponseAnimationVisemeDelta>
    {
        /// <summary>
        ///     The event type associated with this handler.
        /// </summary>
        public static string EventType = ResponseAnimationVisemeDelta.Type;

        /// <summary>
        ///     Gets the message type handled by this handler.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Occurs when a message of type <see cref="ResponseAnimationVisemeDelta" /> is processed.
        /// </summary>
        public override event Action<ResponseAnimationVisemeDelta> OnProcessMessage = null;

        /// <summary>
        ///     Handles the incoming message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ResponseAnimationVisemeDelta>();
            if (json == null)
                throw new InvalidOperationException("Deserialization failed for ResponseAnimationVisemeDelta.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}