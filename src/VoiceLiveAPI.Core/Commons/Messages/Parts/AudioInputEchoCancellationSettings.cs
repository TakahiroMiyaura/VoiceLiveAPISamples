// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the settings for echo cancellation applied to the audio input.
    /// </summary>
    public class AudioInputEchoCancellationSettings
    {
        /// <summary>
        ///     Gets or sets the type of echo cancellation applied to the audio input (e.g.
        ///     <c>server_echo_cancellation</c>).
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the source of the echo cancellation reference signal: <c>server</c> (default;
        ///     internal TTS loopback) or <c>client</c> (channel 1 of the stereo input). Available in API
        ///     version 2026-06-01-preview and later; <c>client</c> requires the <c>client_ec_reference</c>
        ///     preview feature flag and <c>channels = 2</c>. Null omits the field (server default).
        /// </summary>
        [JsonPropertyName("reference_source")]
        public string ReferenceSource { get; set; } = null;

        /// <summary>
        ///     Gets or sets the number of input audio channels: <c>1</c> (mono, default) or <c>2</c>
        ///     (interleaved stereo PCM16 where channel 0 is the microphone and channel 1 is the echo
        ///     reference). <c>2</c> requires <c>reference_source = client</c> and <c>input_audio_format =
        ///     pcm16</c>. Null omits the field (server default = mono).
        /// </summary>
        [JsonPropertyName("channels")]
        public int? Channels { get; set; } = null;
    }
}