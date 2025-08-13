// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>
/// Represents information about a conversation object.
/// </summary>
public class ConversationObjectInfo
{
    /// <summary>
    /// Gets or sets the unique ID of the conversation.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// The object type must be realtime.conversation.
    /// </summary>
    [JsonPropertyName("object")]
    public string? objectInfo { get; set; }
}
