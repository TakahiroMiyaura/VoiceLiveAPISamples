// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified
{
    /// <summary>
    ///     Represents the details of output tokens, including text and audio tokens.
    /// </summary>
    public class OutputTokenDetails
    {
        /// <summary>
        ///     Gets or sets the number of text tokens.
        /// </summary>
        [JsonPropertyName("text_tokens")]
        public int TextTokens { get; set; }

        /// <summary>
        ///     Gets or sets the number of audio tokens.
        /// </summary>
        [JsonPropertyName("audio_tokens")]
        public int AudioTokens { get; set; }
    }
}