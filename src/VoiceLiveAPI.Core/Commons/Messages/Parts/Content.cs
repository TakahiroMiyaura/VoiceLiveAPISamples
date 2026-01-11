// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the content of a message, including its type and transcript.
    /// </summary>
    public class Content
    {
        /// <summary>
        ///     Gets or sets the type of the content.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the transcript of the content.
        /// </summary>
        [JsonPropertyName("transcript")]
        public string Transcript { get; set; } = null;
    }
}