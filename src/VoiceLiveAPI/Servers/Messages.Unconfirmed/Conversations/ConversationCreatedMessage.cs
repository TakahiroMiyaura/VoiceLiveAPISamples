// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Conversations;

/// <summary>
///     Represents a conversation created message.
/// </summary>
public class ConversationCreatedMessage : VoiceLiveMessage
{
    /// <summary>
    ///     Gets or sets the conversation object.
    /// </summary>
    public ConversationObjectInfo? conversation { get; set; }
}