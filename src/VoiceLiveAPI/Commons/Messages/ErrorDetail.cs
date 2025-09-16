// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>  
    /// Represents the details of an error.  
    /// </summary>  
    public class ErrorDetail
    {
        /// <summary>  
        /// Gets or sets the error message.  
        /// </summary>  
        public string message { get; set; } = null;

        /// <summary>  
        /// Gets or sets the type of the error.  
        /// </summary>  
        public string type { get; set; } = null;

        /// <summary>  
        /// Gets or sets the error code.  
        /// </summary>  
        public string code { get; set; } = null;

        /// <summary>  
        /// Gets or sets the parameter associated with the error.  
        /// </summary>  
        public string param { get; set; } = null;

        /// <summary>  
        /// Gets or sets the event ID related to the error.  
        /// </summary>  
        public object event_id { get; set; } = null;
    }
}