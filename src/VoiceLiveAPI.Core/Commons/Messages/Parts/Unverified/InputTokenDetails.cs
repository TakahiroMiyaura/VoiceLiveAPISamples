// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified
{
    /// <summary>
    ///     Represents the details of input tokens, including cached, text, and audio tokens.
    /// </summary>
    public class InputTokenDetails
    {
        /// <summary>
        ///     Gets or sets the number of cached tokens.
        /// </summary>
        [JsonPropertyName("cached_tokens")]
        public int? CachedTokens { get; set; }

        /// <summary>
        ///     Gets or sets the number of text tokens.
        /// </summary>
        [JsonPropertyName("text_tokens")]
        public int? TextTokens { get; set; }

        /// <summary>
        ///     Gets or sets the number of audio tokens.
        /// </summary>
        [JsonPropertyName("audio_tokens")]
        public int? AudioTokens { get; set; }

        /// <summary>
        ///     Gets or sets the details of cached tokens.
        /// </summary>
        [JsonPropertyName("cached_tokens_details")]
        public CachedTokensDetails CachedTokensDetails { get; set; } = null;
    }
}