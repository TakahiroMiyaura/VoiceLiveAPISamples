// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the settings for audio noise reduction in audio input.
    /// </summary>
    public class AudioInputAudioNoiseReductionSettings
    {
        /// <summary>
        ///     Gets or sets the type of noise reduction applied to the audio input.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;
    }
}