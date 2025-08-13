// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Conversations.Items;

/// <summary>  
///     Represents a conversation Item retrieved message.  
/// </summary>  
public class ConversationItemRetrievedMessage : VoiceLiveMessage
{
    /// <summary>  
    ///     Gets or sets the event ID associated with the retrieved conversation item.  
    /// </summary>  
    public string? event_id { get; set; } = string.Empty;

    /// <summary>  
    ///     Gets or sets the information about the retrieved conversation item.  
    /// </summary>  
    public ConversationResponseItemInfo? ItemInfo { get; set; }
}
