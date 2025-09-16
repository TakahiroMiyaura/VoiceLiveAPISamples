// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>  
    /// Represents the configuration for turn detection in a voice interaction system.  
    /// </summary>  
    public class TurnDetection
    {
        /// <summary>  
        /// Gets or sets the eagerness level for turn detection.  
        /// </summary>  
        public string eagerness { get; set; } = null;

        /// <summary>  
        /// Gets or sets the type of turn detection.  
        /// </summary>  
        public string type { get; set; } = null;

        /// <summary>  
        /// Gets or sets the threshold value for detecting a turn.  
        /// </summary>  
        public float? threshold { get; set; }

        /// <summary>  
        /// Gets or sets the prefix padding in milliseconds.  
        /// </summary>  
        public int? prefix_padding_ms { get; set; }

        /// <summary>  
        /// Gets or sets the duration of silence in milliseconds to detect the end of a turn.  
        /// </summary>  
        public int? silence_duration_ms { get; set; }

        /// <summary>  
        /// Gets or sets a value indicating whether to create a response after detecting a turn.  
        /// </summary>  
        public bool? create_response { get; set; }

        /// <summary>  
        /// Gets or sets a value indicating whether to interrupt the response when a new turn is detected.  
        /// </summary>  
        public bool? interrupt_response { get; set; }

        /// <summary>  
        /// Gets or sets a value indicating whether to remove filler words during turn detection.  
        /// </summary>  
        public bool? remove_filler_words { get; set; }

        /// <summary>  
        /// Gets or sets the configuration for end-of-utterance detection.  
        /// </summary>  
        public object end_of_utterance_detection { get; set; } = null;
    }
}