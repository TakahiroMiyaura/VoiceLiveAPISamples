// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>
    /// Represents information about a conversation response item.
    /// </summary>
    public class ConversationResponseItemInfo
    {
        /// <summary>
        /// Gets or sets the Item ID.
        /// </summary>
        public string id { get; set; } = null;

        /// <summary>
        /// Gets or sets the type of the item. Possible values: message, function_call, function_call_output.
        /// </summary>
        public string type { get; set; } = null;

        /// <summary>
        /// Gets or sets the Item content.
        /// </summary>
        [JsonPropertyName("object")]
        public string objectInfo { get; set; } = null;
    }
}