// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified
{
    /// <summary>
    ///     Represents information about a conversation object.
    /// </summary>
    public class ConversationObjectInfo
    {
        /// <summary>
        ///     Gets or sets the unique ID of the conversation.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null;

        /// <summary>
        ///     The object type must be realtime.conversation.
        /// </summary>
        [JsonPropertyName("object")]
        public string ObjectInfo { get; set; } = null;
    }
}