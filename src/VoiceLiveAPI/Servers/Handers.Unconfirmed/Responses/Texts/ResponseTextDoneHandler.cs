// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Responses.Texts;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.Responses.Texts
{

    /// <summary>
    ///     Handles response text done messages.
    /// </summary>
    public class ResponseTextDoneHandler : VoiceLiveHandlerBase<ResponseTextDoneMessage>
    {
        /// <summary>
        ///     Gets the event type for response text done.
        /// </summary>
        public static string EventType = "response.text.done";

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Occurs when a response text done message is processed.
        /// </summary>
        public override event Action<ResponseTextDoneMessage> OnProcessMessage = null;

        /// <summary>
        ///     Handles the response text done message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var messageString = message.GetRawText();
            var responseTextDone = JsonSerializer.Deserialize<ResponseTextDoneMessage>(messageString);
            using (var writer = new StreamWriter(new FileStream(EventType + ".txt", FileMode.Create)))
            {
                await writer.WriteAsync(messageString);
                await writer.FlushAsync();
            }

            if (responseTextDone == null)
                throw new InvalidOperationException("Deserialization failed for ResponseTextDoneMessage.");
            OnProcessMessage?.Invoke(responseTextDone);
            await Task.CompletedTask;
        }
    }
}