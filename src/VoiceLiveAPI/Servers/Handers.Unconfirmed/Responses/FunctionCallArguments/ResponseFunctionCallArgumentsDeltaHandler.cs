// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Responses.FunctionCallArguments;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.Responses.FunctionCallArguments
{

    /// <summary>
    ///     Handles response function call arguments delta messages.
    /// </summary>
    public class
        ResponseFunctionCallArgumentsDeltaHandler : VoiceLiveHandlerBase<ResponseFunctionCallArgumentsDeltaMessage>
    {
        /// <summary>
        ///     Gets the event type for response function call arguments delta.
        /// </summary>
        public static string EventType = "response.function_call_arguments.delta";

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Occurs when a response function call arguments delta message is processed.
        /// </summary>
        public override event Action<ResponseFunctionCallArgumentsDeltaMessage> OnProcessMessage = null;

        /// <summary>
        ///     Handles the response function call arguments delta message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var messageString = message.GetRawText();
            var responseFunctionCallArgumentsDelta =
                JsonSerializer.Deserialize<ResponseFunctionCallArgumentsDeltaMessage>(messageString);
            using (var writer =
                   new StreamWriter(new FileStream(EventType + ".txt",
                       FileMode.Create)))
            {
                await writer.WriteAsync(messageString);
                await writer.FlushAsync();
            }

            if (responseFunctionCallArgumentsDelta == null)
                throw new InvalidOperationException(
                    "Deserialization failed for ResponseFunctionCallArgumentsDeltaMessage.");
            OnProcessMessage?.Invoke(responseFunctionCallArgumentsDelta);
            await Task.CompletedTask;
        }
    }
}