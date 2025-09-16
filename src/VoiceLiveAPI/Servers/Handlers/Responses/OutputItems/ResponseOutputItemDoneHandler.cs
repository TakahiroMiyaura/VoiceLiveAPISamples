// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.OutputItems;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses.OutputItems
{

    /// <summary>
    ///     Handles response output Item done messages.
    /// </summary>
    public class ResponseOutputItemDoneHandler : VoiceLiveHandlerBase<ResponseOutputItemDone>
    {
        /// <summary>
        ///     Gets the event type for response output Item done.
        /// </summary>
        public static string EventType = ResponseOutputItemDone.Type;

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Occurs when a response output Item done message is processed.
        /// </summary>
        public override event Action<ResponseOutputItemDone> OnProcessMessage = null;

        /// <summary>
        ///     Handles the response output Item done message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ResponseOutputItemDone>();
            if (json == null)
                throw new InvalidOperationException("Deserialization failed for ResponseOutputItemDoneMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}