// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the usage details including token counts and their details.
    /// </summary>
    public class Usage
    {
        /// <summary>
        ///     Gets or sets the total number of tokens used.
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public int? TotalTokens { get; set; }

        /// <summary>
        ///     Gets or sets the number of input tokens used.
        /// </summary>
        [JsonPropertyName("input_tokens")]
        public int? InputTokens { get; set; }

        /// <summary>
        ///     Gets or sets the number of output tokens used.
        /// </summary>
        [JsonPropertyName("output_tokens")]
        public int? OutputTokens { get; set; }

        /// <summary>
        ///     Gets or sets the details of input tokens.
        /// </summary>
        [JsonPropertyName("input_token_details")]
        public InputTokenDetails InputTokenDetails { get; set; } = null;

        /// <summary>
        ///     Gets or sets the details of output tokens.
        /// </summary>
        [JsonPropertyName("output_token_details")]
        public OutputTokenDetails OutputTokenDetails { get; set; } = null;
    }
}