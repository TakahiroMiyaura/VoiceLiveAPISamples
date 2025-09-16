// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>  
    /// Represents a function with a name, description, and parameters.  
    /// </summary>  
    public class Function
    {
        /// <summary>  
        /// Gets or sets the name of the function to use.  
        /// </summary>  
        public string name { get; set; } = null;

        /// <summary>  
        /// Gets or sets the description of the function.  
        /// </summary>  
        public string description { get; set; } = null;

        /// <summary>  
        /// Gets or sets the parameters of the function.  
        /// </summary>  
        public Params parameters { get; set; } = null;
    }
}