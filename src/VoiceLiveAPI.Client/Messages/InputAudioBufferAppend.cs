// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Messages
{
    /// <summary>
    ///     Represents a message to append audio data to the input audio buffer.
    /// </summary>
    public class InputAudioBufferAppend : MessageBase
    {
        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "input_audio_buffer.append";

        /// <summary>
        ///     Base64-encoded audio bytes. This value must be in the format specified by the input_audio_format field in the
        ///     session configuration.
        /// </summary>
        [JsonPropertyName("audio")]
        public string Audio { get; set; } = string.Empty;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputAudioBufferAppend" /> class.
        /// </summary>
        public InputAudioBufferAppend()
        {
            EventId = Guid.NewGuid().ToString();
            Type = TypeName;
        }

        /// <summary>
        ///     Encodes audio bytes to a Base64 string.
        /// </summary>
        /// <param name="audioBytes">The audio bytes to encode.</param>
        /// <returns>A Base64-encoded string representation of the audio bytes.</returns>
        public static string ConvertToBase64(byte[] audioBytes)
        {
            return Convert.ToBase64String(audioBytes);
        }
    }
}