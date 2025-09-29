// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Messages
{
    /// <summary>
    ///     Represents a conversation Item truncated message.
    /// </summary>
    public class ConversationItemTruncatedMessage : MessageBase
    {
        /// <summary>
        ///     Gets or sets the Item ID.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the content index.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the audio end in milliseconds.
        /// </summary>
        [JsonPropertyName("audio_end_ms")]
        public int? AudioEndMs { get; set; }
    }
}