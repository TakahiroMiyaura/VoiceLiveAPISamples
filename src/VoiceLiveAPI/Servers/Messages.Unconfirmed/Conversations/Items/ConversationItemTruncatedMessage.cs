// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Conversations.Items;

/// <summary>
///     Represents a conversation Item truncated message.
/// </summary>
public class ConversationItemTruncatedMessage : VoiceLiveMessage
{
    /// <summary>
    ///     Gets or sets the Item ID.
    /// </summary>
    public string item_id { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the content index.
    /// </summary>
    public int content_index { get; set; }

    /// <summary>
    ///     Gets or sets the audio end in milliseconds.
    /// </summary>
    public int? audio_end_ms { get; set; }
}