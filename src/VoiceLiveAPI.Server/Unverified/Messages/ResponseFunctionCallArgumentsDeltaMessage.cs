// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Messages
{
    /// <summary>
    ///     Represents a response function call arguments delta message.
    /// </summary>
    public class ResponseFunctionCallArgumentsDeltaMessage : MessageBase
    {
        /// <summary>
        ///     Gets or sets the response ID.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the Item ID.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the output index.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the call ID.
        /// </summary>
        [JsonPropertyName("call_id")]
        public string CallId { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the arguments delta.
        /// </summary>
        [JsonPropertyName("delta")]
        public string Delta { get; set; } = string.Empty;
    }
}