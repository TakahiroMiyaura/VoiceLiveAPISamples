// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified
{
    /// <summary>
    ///     Represents information about a content part, which can include text, audio, or references.
    /// </summary>
    public class ContentPartInfo
    {
        /// <summary>
        ///     Gets or sets the type of the content part.
        ///     Allowed values: input_text, input_audio, item_reference, text.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the text content of the content part.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = null;

        /// <summary>
        ///     Gets or sets the unique identifier of the content part.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null;

        /// <summary>
        ///     Gets or sets the audio content of the content part.
        /// </summary>
        [JsonPropertyName("audio")]
        public string Audio { get; set; } = null;

        /// <summary>
        ///     Gets or sets the transcript of the audio content.
        /// </summary>
        [JsonPropertyName("transcript")]
        public string Transcript { get; set; } = null;
    }
}