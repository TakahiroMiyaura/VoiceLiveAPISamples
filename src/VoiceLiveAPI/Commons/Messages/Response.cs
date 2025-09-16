// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>  
    /// Represents the response from the VoiceLive API.  
    /// </summary>  
    public class Response
    {
        /// <summary>  
        /// Gets or sets the unique identifier of the response.  
        /// </summary>  
        public string id { get; set; } = null;

        /// <summary>  
        /// Gets or sets the status of the response.  
        /// </summary>  
        public string status { get; set; } = null;

        /// <summary>  
        /// Gets or sets the details of the response status.  
        /// </summary>  
        public StatusDetails status_details { get; set; } = null;

        /// <summary>  
        /// Gets or sets the output data of the response.  
        /// </summary>  
        public object[] output { get; set; } = null;

        /// <summary>  
        /// Gets or sets the usage details of the response.  
        /// </summary>  
        public Usage usage { get; set; } = null;
    }
}