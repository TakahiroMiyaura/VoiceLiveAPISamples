// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.InputAudioBuffers
{

    /// <summary>  
    /// Represents a message indicating that speech has started in the input audio buffer.  
    /// </summary>  
    public class InputAudioBufferSpeechStarted : MessageBase
    {
        /// <summary>  
        /// The type identifier for this message.  
        /// </summary>  
        public const string Type = "input_audio_buffer.speech_started";

        /// <summary>  
        /// Gets or sets the start time of the audio in milliseconds.  
        /// </summary>  
        public int? audio_start_ms { get; set; }

        /// <summary>  
        /// Gets or sets the identifier for the item associated with this message.  
        /// </summary>  
        public string item_id { get; set; } = null;

        /// <summary>  
        /// Initializes a new instance of the <see cref="InputAudioBufferSpeechStarted"/> class.  
        /// </summary>  
        public InputAudioBufferSpeechStarted()
        {
            type = Type;
        }
    }
}
