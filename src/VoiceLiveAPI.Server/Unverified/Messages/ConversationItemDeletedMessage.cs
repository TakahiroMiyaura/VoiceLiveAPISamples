// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Messages
{
    /// <summary>
    ///     Represents a conversation Item deleted message.
    /// </summary>
    public class ConversationItemDeletedMessage : MessageBase
    {
        /// <summary>
        ///     Gets or sets the deleted Item ID.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; } = string.Empty;
    }
}