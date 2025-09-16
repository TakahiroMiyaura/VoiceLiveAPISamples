// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.OutputAudioBuffers
{

    /// <summary>  
    ///     Represents an output audio buffer cleared message.  
    /// </summary>  
    public class OutputAudioBufferClearedMessage : VoiceLiveMessage
    {
        /// <summary>  
        ///     Gets or sets the event identifier.  
        /// </summary>  
        public string event_id { get; set; } = null;

        /// <summary>  
        ///     Gets or sets the response identifier.  
        /// </summary>  
        public string response_id { get; set; } = null;
    }
}