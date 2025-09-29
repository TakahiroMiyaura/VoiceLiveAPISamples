// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the configuration for turn detection in a voice interaction system.
    /// </summary>
    public class TurnDetection
    {
        /// <summary>
        ///     Gets or sets the eagerness level for turn detection.
        /// </summary>
        [JsonPropertyName("eagerness")]
        public string Eagerness { get; set; } = null;

        /// <summary>
        ///     Gets or sets the type of turn detection.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the threshold value for detecting a turn.
        /// </summary>
        [JsonPropertyName("threshold")]
        public float? Threshold { get; set; }

        /// <summary>
        ///     Gets or sets the prefix padding in milliseconds.
        /// </summary>
        [JsonPropertyName("prefix_padding_ms")]
        public int? PrefixPaddingMs { get; set; }

        /// <summary>
        ///     Gets or sets the duration of silence in milliseconds to detect the end of a turn.
        /// </summary>
        [JsonPropertyName("silence_duration_ms")]
        public int? SilenceDurationMs { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to create a response after detecting a turn.
        /// </summary>
        [JsonPropertyName("create_response")]
        public bool? CreateResponse { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to interrupt the response when a new turn is detected.
        /// </summary>
        [JsonPropertyName("interrupt_response")]
        public bool? InterruptResponse { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to remove filler words during turn detection.
        /// </summary>
        [JsonPropertyName("remove_filler_words")]
        public bool? RemoveFillerWords { get; set; }

        /// <summary>
        ///     Gets or sets the configuration for end-of-utterance detection.
        /// </summary>
        [JsonPropertyName("end_of_utterance_detection")]
        public object EndOfUtteranceDetection { get; set; } = null;
    }
}