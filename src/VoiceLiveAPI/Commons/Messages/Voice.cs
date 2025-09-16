// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>  
    /// Represents a voice configuration with various properties such as name, type, and customization options.  
    /// </summary>  
    public class Voice
    {
        /// <summary>  
        /// Gets or sets the name of the voice.  
        /// </summary>  
        public string name { get; set; } = null;

        /// <summary>  
        /// Gets or sets the type of the voice.  
        /// </summary>  
        public string type { get; set; } = null;

        /// <summary>  
        /// Gets or sets the temperature value for the voice configuration.  
        /// </summary>  
        public object temperature { get; set; } = null;

        /// <summary>  
        /// Gets or sets the URL for a custom lexicon.  
        /// </summary>  
        public object custom_lexicon_url { get; set; } = null;

        /// <summary>  
        /// Gets or sets the preferred locales for the voice.  
        /// </summary>  
        public object prefer_locales { get; set; } = null;

        /// <summary>  
        /// Gets or sets the style of the voice.  
        /// </summary>  
        public object style { get; set; } = null;

        /// <summary>  
        /// Gets or sets the pitch of the voice.  
        /// </summary>  
        public object pitch { get; set; } = null;

        /// <summary>  
        /// Gets or sets the rate of speech for the voice.  
        /// </summary>  
        public object rate { get; set; } = null;

        /// <summary>  
        /// Gets or sets the volume of the voice.  
        /// </summary>  
        public object volume { get; set; } = null;
    }
}