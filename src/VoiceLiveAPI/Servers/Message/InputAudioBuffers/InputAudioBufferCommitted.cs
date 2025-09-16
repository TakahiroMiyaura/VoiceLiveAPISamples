// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.InputAudioBuffers
{

    /// <summary>  
    /// Represents a message indicating that an input audio buffer has been committed.  
    /// </summary>  
    public class InputAudioBufferCommitted : MessageBase
    {
        /// <summary>  
        /// The type identifier for this message.  
        /// </summary>  
        public const string Type = "input_audio_buffer.committed";

        /// <summary>  
        /// Gets or sets the identifier of the previous item in the buffer.  
        /// </summary>  
        public object previous_item_id { get; set; }

        /// <summary>  
        /// Gets or sets the identifier of the current item in the buffer.  
        /// </summary>  
        public string item_id { get; set; }

        /// <summary>  
        /// Initializes a new instance of the <see cref="InputAudioBufferCommitted"/> class.  
        /// </summary>  
        public InputAudioBufferCommitted()
        {
            event_id = Guid.NewGuid().ToString();
            type = Type;
            previous_item_id = string.Empty;
            item_id = string.Empty;
        }
    }
}