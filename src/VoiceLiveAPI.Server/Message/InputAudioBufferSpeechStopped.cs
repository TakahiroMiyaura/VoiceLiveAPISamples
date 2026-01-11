// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a message indicating that speech has stopped in the input audio buffer.
    /// </summary>
    public class InputAudioBufferSpeechStopped : MessageBase
    {
        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "input_audio_buffer.speech_stopped";

        /// <summary>
        ///     Gets or sets the timestamp (in milliseconds) indicating the end of the audio.
        /// </summary>
        [JsonPropertyName("audio_end_ms")]
        public int AudioEndMs { get; set; }

        /// <summary>
        ///     Gets or sets the identifier for the audio item.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputAudioBufferSpeechStopped" /> class.
        /// </summary>
        public InputAudioBufferSpeechStopped()
        {
            EventId = string.Empty;
            Type = TypeName;
            AudioEndMs = 0;
            ItemId = string.Empty;
        }
    }
}