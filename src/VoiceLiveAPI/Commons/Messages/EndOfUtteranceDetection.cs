// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>  
    /// Represents the configuration for end-of-utterance detection.  
    /// </summary>  
    public class EndOfUtteranceDetection
    {
        /// <summary>  
        /// Gets or sets the model used for end-of-utterance detection.  
        /// </summary>  
        public string model { get; set; }

        /// <summary>  
        /// Gets or sets the primary threshold for detection.  
        /// </summary>  
        public float threshold { get; set; }

        /// <summary>  
        /// Gets or sets the timeout value in milliseconds for the primary detection.  
        /// </summary>  
        public int timeout { get; set; }

        /// <summary>  
        /// Gets or sets the secondary threshold for detection.  
        /// </summary>  
        public float secondary_threshold { get; set; }

        /// <summary>  
        /// Gets or sets the timeout value in milliseconds for the secondary detection.  
        /// </summary>  
        public int secondary_timeout { get; set; }
    }
}