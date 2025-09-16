// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Sessions;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Sessions
{

    /// <summary>
    ///     Handles session update messages for the VoiceLive API.
    /// </summary>
    public class ServerSessionUpdateHandler : VoiceLiveHandlerBase<ServerSessionUpdated>
    {
        /// <summary>
        ///     The event type for session updates.
        /// </summary>
        public static string EventType = ServerSessionUpdated.Type;

        /// <summary>
        ///     Gets the message type associated with this handler.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Occurs when a session update message is processed.
        /// </summary>
        public override event Action<ServerSessionUpdated> OnProcessMessage = null;

        /// <summary>
        ///     Handles the incoming session update message asynchronously.
        /// </summary>
        /// <param name="message">The JSON element containing the session update message.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization of the message fails.</exception>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ServerSessionUpdated>();
            if (json == null)
                throw new InvalidOperationException("Deserialization failed for SessionUpdateMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}