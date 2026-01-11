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
    ///     Handles Session update messages for the VoiceLive API.
    /// </summary>
    [Obsolete(
        "This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers.ServerSessionUpdateHandler instead.")]
    public class ServerSessionUpdateHandler : VoiceLiveHandlerBase<ServerSessionUpdated>
    {
        /// <summary>
        ///     The event type for Session updates.
        /// </summary>
        public const string EventType = ServerSessionUpdated.TypeName;

        /// <summary>
        ///     Gets the message type associated with this handler.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } = LoggerFactoryManager.CreateLogger<ServerSessionUpdateHandler>();

        /// <summary>
        ///     Occurs when a Session update message is processed.
        /// </summary>
        public override event Action<ServerSessionUpdated> OnProcessMessage;

        /// <summary>
        ///     Handles the incoming Session update message asynchronously.
        /// </summary>
        /// <param name="message">The JSON element containing the Session update message.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization of the message fails.</exception>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ServerSessionUpdated>() ??
                       throw new InvalidOperationException("Deserialization failed for SessionUpdateMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}