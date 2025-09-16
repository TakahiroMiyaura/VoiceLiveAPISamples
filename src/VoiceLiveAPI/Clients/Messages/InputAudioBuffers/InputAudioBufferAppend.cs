// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.InputAudioBuffers
{

    /// <summary>  
    /// Represents a message to append audio data to the input audio buffer.  
    /// </summary>  
    public class InputAudioBufferAppend : MessageBase
    {
        /// <summary>  
        /// The type identifier for this message.  
        /// </summary>  
        public static string Type = "input_audio_buffer.append";

        /// <summary>  
        /// Base64-encoded audio bytes. This value must be in the format specified by the input_audio_format field in the  
        /// session configuration.  
        /// </summary>  
        public string audio { get; set; } = string.Empty;

        /// <summary>  
        /// Initializes a new instance of the <see cref="InputAudioBufferAppend"/> class.  
        /// </summary>  
        public InputAudioBufferAppend()
        {
            event_id = Guid.NewGuid().ToString();
            type = Type;
        }

        /// <summary>  
        /// Encodes audio bytes to a Base64 string.  
        /// </summary>  
        /// <param name="audioBytes">The audio bytes to encode.</param>  
        /// <returns>A Base64-encoded string representation of the audio bytes.</returns>  
        public static string ConvertToBase64(byte[] audioBytes)
        {
            return Convert.ToBase64String(audioBytes);
        }
    }
}