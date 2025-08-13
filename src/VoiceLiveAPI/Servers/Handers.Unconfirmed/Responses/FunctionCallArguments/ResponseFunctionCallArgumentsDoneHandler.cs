// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Responses.FunctionCallArguments;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.Responses.FunctionCallArguments;

/// <summary>
///     Handles response function call arguments done messages.
/// </summary>
public class ResponseFunctionCallArgumentsDoneHandler : VoiceLiveHandlerBase<ResponseFunctionCallArgumentsDoneMessage>
{
    /// <summary>
    ///     Gets the event type for response function call arguments done.
    /// </summary>
    public static string EventType = "response.function_call_arguments.done";

    /// <summary>
    ///     Gets the message type this handler can process.
    /// </summary>
    public override string MessageType => EventType;

    /// <summary>
    ///     Occurs when a response function call arguments done message is processed.
    /// </summary>
    public override event Action<ResponseFunctionCallArgumentsDoneMessage>? OnProcessMessage;

    /// <summary>
    ///     Handles the response function call arguments done message asynchronously.
    /// </summary>
    /// <param name="message">The JSON message to handle.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task HandleAsync(JsonElement message)
    {
        var messageString = message.GetRawText();
        var responseFunctionCallArgumentsDone =
            JsonSerializer.Deserialize<ResponseFunctionCallArgumentsDoneMessage>(messageString);
        await using (var writer =
                     new StreamWriter(new FileStream(EventType + ".txt", FileMode.Create)))
        {
            await writer.WriteAsync(messageString);
            await writer.FlushAsync();
        }

        if (responseFunctionCallArgumentsDone == null)
            throw new InvalidOperationException("Deserialization failed for ResponseFunctionCallArgumentsDoneMessage.");
        OnProcessMessage?.Invoke(responseFunctionCallArgumentsDone);
        await Task.CompletedTask;
    }
}