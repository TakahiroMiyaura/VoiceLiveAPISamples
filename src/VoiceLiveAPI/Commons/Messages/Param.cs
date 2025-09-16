// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>  
    /// Represents a parameter with a type and description.  
    /// </summary>  
    public class Param
    {
        /// <summary>  
        /// Gets or sets the type of the parameter.  
        /// </summary>  
        public string type { get; set; } = null;

        /// <summary>  
        /// Gets or sets the description of the parameter.  
        /// </summary>  
        public string description { get; set; } = null;
    }
}
