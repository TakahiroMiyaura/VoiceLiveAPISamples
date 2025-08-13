// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Conversations.Items;

/// <summary>
///     Represents a conversation Item input audio transcription failed message.
/// </summary>
public class ConversationItemInputAudioTranscriptionFailedMessage : VoiceLiveMessage
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
    ///     Gets or sets the error information.
    /// </summary>
    public ErrorInfo? error { get; set; }
}