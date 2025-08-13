// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Sessions;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Sessions;

/// <summary>
///     Handles session created messages.
/// </summary>
public class SessionCreatedHandler : VoiceLiveHandlerBase<SessionCreated>
{
    /// <summary>
    ///     Gets the event type for session created.
    /// </summary>
    public static string EventType = SessionCreated.Type;

    /// <summary>
    ///     Gets the message type this handler can process.
    /// </summary>
    public override string MessageType => EventType;

    /// <summary>
    ///     Occurs when a session created message is processed.
    /// </summary>
    public override event Action<SessionCreated>? OnProcessMessage;

    /// <summary>
    ///     Handles the session created message asynchronously.
    /// </summary>
    /// <param name="message">The JSON message to handle.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task HandleAsync(JsonElement message)
    {
        var json = message.Deserialize<SessionCreated>();
        if (json == null)
            throw new InvalidOperationException("Deserialization failed for SessionCreatedMessage.");
        OnProcessMessage?.Invoke(json);
        await Task.CompletedTask;
    }
}