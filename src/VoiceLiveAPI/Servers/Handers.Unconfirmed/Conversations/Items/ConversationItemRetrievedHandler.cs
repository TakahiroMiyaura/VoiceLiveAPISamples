// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Conversations.Items;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.Conversations.Items;

/// <summary>
///     Handles conversation Item retrieved messages.
/// </summary>
public class ConversationItemRetrievedHandler : VoiceLiveHandlerBase<ConversationItemRetrievedMessage>
{
    /// <summary>
    ///     Gets the event type for conversation Item retrieved.
    /// </summary>
    public static string EventType = "conversation.Item.retrieved";

    /// <summary>
    ///     Gets the message type this handler can process.
    /// </summary>
    public override string MessageType => EventType;

    /// <summary>
    ///     Occurs when a conversation Item retrieved message is processed.
    /// </summary>
    public override event Action<ConversationItemRetrievedMessage>? OnProcessMessage;

    /// <summary>
    ///     Handles the conversation Item retrieved message asynchronously.
    /// </summary>
    /// <param name="message">The JSON message to handle.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task HandleAsync(JsonElement message)
    {
        var messageString = message.GetRawText();
        var conversationItemRetrieved = JsonSerializer.Deserialize<ConversationItemRetrievedMessage>(messageString);
        await using (var writer =
                     new StreamWriter(new FileStream(EventType + ".txt", FileMode.Create)))
        {
            await writer.WriteAsync(messageString);
            await writer.FlushAsync();
        }

        if (conversationItemRetrieved == null)
            throw new InvalidOperationException("Deserialization failed for ConversationItemRetrievedMessage.");
        OnProcessMessage?.Invoke(conversationItemRetrieved);
        await Task.CompletedTask;
    }
}