// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.Audios;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses.Audios;

/// <summary>
///     Handles messages of type "response.audio.delta".
/// </summary>
public class ResponseAudioDeltaHandler : VoiceLiveHandlerBase<ResponseAudioDelta>
{
    /// <summary>
    ///     The event type associated with this handler.
    /// </summary>
    public static string EventType = ResponseAudioDelta.Type;

    /// <summary>
    ///     Gets the message type handled by this handler.
    /// </summary>
    public override string MessageType => EventType;

    /// <summary>
    ///     Occurs when a message of type <see cref="ResponseAudioDelta" /> is processed.
    /// </summary>
    public override event Action<ResponseAudioDelta>? OnProcessMessage;

    /// <summary>
    ///     Handles the incoming message asynchronously.
    /// </summary>
    /// <param name="message">The JSON message to handle.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
    public override async Task HandleAsync(JsonElement message)
    {
        var json = message.Deserialize<ResponseAudioDelta>();
        if (json == null)
            throw new InvalidOperationException("Deserialization failed for ResponseAudioDeltaMessage.");
        OnProcessMessage?.Invoke(json);
        await Task.CompletedTask;
    }
}