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
    ///     Handler for processing <see cref="VisemeDelta" /> messages in the VoiceLive API server.
    /// </summary>
    public class ResponseAnimationVisemeDeltaHandler : VoiceLiveHandlerBase<VisemeDelta>
    {
        /// <summary>
        ///     The event type associated with this handler.
        /// </summary>
        public const string EventType = VisemeDelta.TypeName;

        /// <summary>
        ///     Gets the message type that this handler processes.
        /// </summary>
        public override string MessageType => VisemeDelta.TypeName;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ResponseAnimationVisemeDeltaHandler>();

        /// <summary>
        ///     Occurs when a <see cref="VisemeDelta" /> message is processed.
        /// </summary>
        public override event Action<VisemeDelta> OnProcessMessage;

        /// <summary>
        ///     Handles the incoming <see cref="VisemeDelta" /> message asynchronously.
        /// </summary>
        /// <param name="message">The message to handle, represented as a <see cref="JsonElement" />.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<VisemeDelta>() ??
                       throw new InvalidOperationException(
                           "Deserialization failed for VisemeDelta.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}