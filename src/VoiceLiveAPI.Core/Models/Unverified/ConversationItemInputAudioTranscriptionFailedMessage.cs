// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Unverified
{
    /// <summary>
    ///     Represents a conversation Item input audio transcription failed message.
    /// </summary>
    public class ConversationItemInputAudioTranscriptionFailedMessage : MessageBase
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
        ///     Gets or sets the error information.
        /// </summary>
        [JsonPropertyName("error")]
        public ErrorInfo Error { get; set; } = null;
    }
}