// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.ContentParts;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses.ContentParts;

/// <summary>
///     Handles response content part added messages.
/// </summary>
public class ResponseContentPartAddedHandler : VoiceLiveHandlerBase<ResponseContentPartAdded>
{
    /// <summary>
    ///     Gets the event type for response content part added.
    /// </summary>
    public static string EventType = ResponseContentPartAdded.Type;

    /// <summary>
    ///     Gets the message type this handler can process.
    /// </summary>
    public override string MessageType => EventType;

    /// <summary>
    ///     Occurs when a response content part added message is processed.
    /// </summary>
    public override event Action<ResponseContentPartAdded>? OnProcessMessage;

    /// <summary>
    ///     Handles the response content part added message asynchronously.
    /// </summary>
    /// <param name="message">The JSON message to handle.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task HandleAsync(JsonElement message)
    {
        var json = message.Deserialize<ResponseContentPartAdded>();
        if (json == null)
            throw new InvalidOperationException("Deserialization failed for ResponseContentPartAddedMessage.");
        OnProcessMessage?.Invoke(json);
        await Task.CompletedTask;
    }
}