// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a client session configuration for the VoiceLiveAPI.
    /// </summary>
    public class ClientSession
    {
        /// <summary>
        ///     Gets or sets the modalities supported by the client session.
        /// </summary>
        [JsonPropertyName("modalities")]
        public string[] Modalities { get; set; } = null;

        /// <summary>
        ///     Gets or sets the input audio format. Supported formats: pcm16, g711_ulaw, g711_alaw.
        /// </summary>
        [JsonPropertyName("input_audio_format")]
        public string InputAudioFormat { get; set; } = null;

        /// <summary>
        ///     Gets or sets the output audio format. Supported formats: pcm16, g711_ulaw, g711_alaw.
        /// </summary>
        [JsonPropertyName("output_audio_format")]
        public string OutputAudioFormat { get; set; } = null;

        /// <summary>
        ///     Gets or sets the settings for input audio noise reduction.
        /// </summary>
        [JsonPropertyName("input_audio_noise_reduction")]
        public AudioInputAudioNoiseReductionSettings InputAudioNoiseReduction { get; set; } = null;

        /// <summary>
        ///     Gets or sets the settings for input audio transcription.
        /// </summary>
        [JsonPropertyName("input_audio_transcription")]
        public AudioInputTranscriptionSettings InputAudioTranscription { get; set; } = null;

        /// <summary>
        ///     Gets or sets the settings for input audio echo cancellation.
        /// </summary>
        [JsonPropertyName("input_audio_echo_cancellation")]
        public AudioInputEchoCancellationSettings InputAudioEchoCancellation { get; set; } = null;

        /// <summary>
        ///     Gets or sets the configuration for turn detection.
        /// </summary>
        [JsonPropertyName("turn_detection")]
        public TurnDetection TurnDetection { get; set; } = null;

        /// <summary>
        ///     Gets or sets the tools available for the client session.
        /// </summary>
        [JsonPropertyName("tools")]
        public Function[] Tools { get; set; } = null;

        /// <summary>
        ///     Gets or sets the tool choice for the client session.
        /// </summary>
        [JsonPropertyName("tool_choice")]
        public string ToolChoice { get; set; } = null;

        /// <summary>
        ///     Gets or sets the maximum number of response output tokens. Can be an integer or "inf".
        /// </summary>
        [JsonPropertyName("max_response_output_tokens")]
        public string MaxResponseOutputTokens { get; set; } = null;

        /// <summary>
        ///     Gets or sets the model used for the client session.
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = null;

        /// <summary>
        ///     Gets or sets the voice configuration for the client session.
        /// </summary>
        [JsonPropertyName("voice")]
        public Voice Voice { get; set; } = null;

        /// <summary>
        ///     Gets or sets the animation settings for the client session.
        /// </summary>
        [JsonPropertyName("animation")]
        public Animation Animation { get; set; } = null;

        /// <summary>
        ///     Gets or sets the avatar configuration for the client session.
        /// </summary>
        [JsonPropertyName("avatar")]
        public Avatar Avatar { get; set; } = null;

        /// <summary>
        ///     Gets or sets the types of output audio timestamps.
        /// </summary>
        [JsonPropertyName("output_audio_timestamp_types")]
        public string[] OutputAudioTimestampTypes { get; set; } = null;

        /// <summary>
        ///     Gets or sets the input audio sampling rate.
        /// </summary>
        [JsonPropertyName("input_audio_sampling_rate")]
        public int? InputAudioSamplingRate { get; set; }
    }
}