// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Conversations.Items;

/// <summary>
///     Represents a conversation Item deleted message.
/// </summary>
public class ConversationItemDeletedMessage : VoiceLiveMessage
{
    /// <summary>
    ///     Gets or sets the deleted Item ID.
    /// </summary>
    public string item_id { get; set; } = string.Empty;
}