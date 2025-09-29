// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a message indicating that speech has started in the input audio buffer.
    /// </summary>
    public class InputAudioBufferSpeechStarted : MessageBase
    {
        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "input_audio_buffer.speech_started";

        /// <summary>
        ///     Gets or sets the start time of the audio in milliseconds.
        /// </summary>
        [JsonPropertyName("audio_start_ms")]
        public int? AudioStartMs { get; set; }

        /// <summary>
        ///     Gets or sets the identifier for the item associated with this message.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; } = null;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputAudioBufferSpeechStarted" /> class.
        /// </summary>
        public InputAudioBufferSpeechStarted()
        {
            Type = TypeName;
        }
    }
}