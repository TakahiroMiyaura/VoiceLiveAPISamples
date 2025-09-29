// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the settings for audio input transcription.
    /// </summary>
    public class AudioInputTranscriptionSettings
    {
        /// <summary>
        ///     Gets or sets the list of phrases to assist the transcription model.
        /// </summary>
        [JsonPropertyName("phrase_list")]
        public string[] PhraseList = null;

        /// <summary>
        ///     Gets or sets the transcription model to be used.
        ///     Supported values: "whisper-1", "gpt-4o-transcribe", "gpt-4o-mini-transcribe".
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = null;

        /// <summary>
        ///     Gets or sets a value indicating whether a custom model is used.
        /// </summary>
        [JsonPropertyName("custom_model")]
        public bool? CustomModel { get; set; }
    }
}