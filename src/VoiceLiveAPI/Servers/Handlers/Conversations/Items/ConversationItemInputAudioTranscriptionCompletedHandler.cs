// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Conversations.Items;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Conversations.Items;

/// <summary>
///     Handles messages of type ConversationItemInputAudioTranscriptionCompletedMessage.
/// </summary>
public class
    ConversationItemInputAudioTranscriptionCompletedHandler :
    VoiceLiveHandlerBase<ConversationItemInputAudioTranscriptionCompleted>, IVoiceLiveHandler
{
    /// <summary>
    ///     The event type associated with this handler.
    /// </summary>
    public static string EventType = ConversationItemInputAudioTranscriptionCompleted.Type;

    /// <summary>
    ///     Gets the message type this handler can process.
    /// </summary>
    public override string MessageType => EventType;

    /// <summary>
    ///     Handles the incoming message asynchronously.
    /// </summary>
    /// <param name="message">The JSON message to process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task HandleAsync(JsonElement message)
    {
        var conversationItemTranscription =
            message.Deserialize<ConversationItemInputAudioTranscriptionCompleted>();
        if (conversationItemTranscription == null)
            throw new InvalidOperationException(
                "Deserialization failed for ConversationItemInputAudioTranscriptionCompletedMessage.");
        OnProcessMessage?.Invoke(conversationItemTranscription);
        await Task.CompletedTask;
    }

    /// <summary>
    ///     Event triggered when a message of type ConversationItemInputAudioTranscriptionCompletedMessage is processed.
    /// </summary>
    public override event Action<ConversationItemInputAudioTranscriptionCompleted>? OnProcessMessage;
}