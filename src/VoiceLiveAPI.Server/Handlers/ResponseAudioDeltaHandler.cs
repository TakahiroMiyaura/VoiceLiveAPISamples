// Copyright (c) 2025 Takahiro Miyaura
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
    ///     Handles messages of type "response.audio.delta".
    /// </summary>
    public class ResponseAudioDeltaHandler : VoiceLiveHandlerBase<ResponseAudioDelta>
    {
        /// <summary>
        ///     The event type associated with this handler.
        /// </summary>
        public const string EventType = ResponseAudioDelta.TypeName;

        /// <summary>
        ///     Gets the message type handled by this handler.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } = LoggerFactoryManager.CreateLogger<ResponseAudioDeltaHandler>();

        /// <summary>
        ///     Occurs when a message of type <see cref="ResponseAudioDelta" /> is processed.
        /// </summary>
        public override event Action<ResponseAudioDelta> OnProcessMessage;

        /// <summary>
        ///     Handles the incoming message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ResponseAudioDelta>() ??
                       throw new InvalidOperationException("Deserialization failed for ResponseAudioDeltaMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}