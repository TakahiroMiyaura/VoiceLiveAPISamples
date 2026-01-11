// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a part of a message with a type and transcript.
    /// </summary>
    public class Part
    {
        /// <summary>
        ///     Gets or sets the type of the part.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the transcript of the part.
        /// </summary>
        [JsonPropertyName("transcript")]
        public string Transcript { get; set; } = null;
    }
}