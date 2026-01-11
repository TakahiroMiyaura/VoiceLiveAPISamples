// Copyright (c) 2026 Takahiro Miyaura
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
    ///     Handler for processing <see cref="ResponseAnimationVisemeDelta" /> messages in the VoiceLive API server.
    /// </summary>
    [Obsolete(
        "This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers.ResponseAnimationVisemeDeltaHandler instead.")]
    public class ResponseAnimationVisemeDeltaHandler : VoiceLiveHandlerBase<ResponseAnimationVisemeDelta>
    {
        /// <summary>
        ///     The event type associated with this handler.
        /// </summary>
        public const string EventType = ResponseAnimationVisemeDelta.TypeName;

        /// <summary>
        ///     Gets the message type that this handler processes.
        /// </summary>
        public override string MessageType => ResponseAnimationVisemeDelta.TypeName;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ResponseAnimationVisemeDeltaHandler>();

        /// <summary>
        ///     Occurs when a <see cref="ResponseAnimationVisemeDelta" /> message is processed.
        /// </summary>
        public override event Action<ResponseAnimationVisemeDelta> OnProcessMessage;

        /// <summary>
        ///     Handles the incoming <see cref="ResponseAnimationVisemeDelta" /> message asynchronously.
        /// </summary>
        /// <param name="message">The message to handle, represented as a <see cref="JsonElement" />.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ResponseAnimationVisemeDelta>() ??
                       throw new InvalidOperationException(
                           "Deserialization failed for ResponseAnimationVisemeDeltaMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}