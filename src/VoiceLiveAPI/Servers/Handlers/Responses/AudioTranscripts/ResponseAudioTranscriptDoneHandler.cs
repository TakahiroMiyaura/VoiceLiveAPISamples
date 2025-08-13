// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.AudioTranscripts;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses.AudioTranscripts;

/// <summary>
///     Handles response audio transcript done messages.
/// </summary>
public class ResponseAudioTranscriptDoneHandler : VoiceLiveHandlerBase<ResponseAudioTranscriptDone>
{
    /// <summary>
    ///     Gets the event type for response audio transcript done.
    /// </summary>
    public static string EventType = ResponseAudioTranscriptDone.Type;

    /// <summary>
    ///     Gets the message type this handler can process.
    /// </summary>
    public override string MessageType => EventType;

    /// <summary>
    ///     Occurs when a response audio transcript done message is processed.
    /// </summary>
    public override event Action<ResponseAudioTranscriptDone>? OnProcessMessage;

    /// <summary>
    ///     Handles the response audio transcript done message asynchronously.
    /// </summary>
    /// <param name="message">The JSON message to handle.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task HandleAsync(JsonElement message)
    {
        var json = message.Deserialize<ResponseAudioTranscriptDone>();
        if (json == null)
            throw new InvalidOperationException("Deserialization failed for ResponseAudioTranscriptDoneMessage.");
        OnProcessMessage?.Invoke(json);
        await Task.CompletedTask;
    }
}