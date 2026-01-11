// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified
{
    /// <summary>
    ///     Represents information about a conversation response item.
    /// </summary>
    public class ConversationResponseItemInfo
    {
        /// <summary>
        ///     Gets or sets the Item ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null;

        /// <summary>
        ///     Gets or sets the type of the item. Possible values: message, function_call, function_call_output.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the Item content.
        /// </summary>
        [JsonPropertyName("object")]
        public string ObjectInfo { get; set; } = null;
    }
}