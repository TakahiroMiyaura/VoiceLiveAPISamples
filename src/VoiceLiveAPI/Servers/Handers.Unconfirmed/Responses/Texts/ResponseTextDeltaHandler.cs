// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Responses.Texts;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.Responses.Texts;

/// <summary>
///     Handles response text delta messages.
/// </summary>
public class ResponseTextDeltaHandler : VoiceLiveHandlerBase<ResponseTextDeltaMessage>
{
    /// <summary>
    ///     Gets the event type for response text delta.
    /// </summary>
    public static string EventType = "response.text.delta";

    /// <summary>
    ///     Gets the message type this handler can process.
    /// </summary>
    public override string MessageType => EventType;

    /// <summary>
    ///     Occurs when a response text delta message is processed.
    /// </summary>
    public override event Action<ResponseTextDeltaMessage>? OnProcessMessage;

    /// <summary>
    ///     Handles the response text delta message asynchronously.
    /// </summary>
    /// <param name="message">The JSON message to handle.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task HandleAsync(JsonElement message)
    {
        var messageString = message.GetRawText();
        var responseTextDelta = JsonSerializer.Deserialize<ResponseTextDeltaMessage>(messageString);
        await using (var writer = new StreamWriter(new FileStream(EventType + ".txt", FileMode.Create)))
        {
            await writer.WriteAsync(messageString);
            await writer.FlushAsync();
        }

        if (responseTextDelta == null)
            throw new InvalidOperationException("Deserialization failed for ResponseTextDeltaMessage.");
        OnProcessMessage?.Invoke(responseTextDelta);
        await Task.CompletedTask;
    }
}