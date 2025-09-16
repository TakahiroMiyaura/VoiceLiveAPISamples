// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses
{

    /// <summary>  
    /// Represents a message indicating that a response has been created.  
    /// </summary>  
    public class ResponseCreated : MessageBase
    {
        /// <summary>  
        /// The type identifier for the "response.created" message.  
        /// </summary>  
        public const string Type = "response.created";

        /// <summary>  
        /// Gets or sets the response associated with this message.  
        /// </summary>  
        public Response response { get; set; }

        /// <summary>  
        /// Initializes a new instance of the <see cref="ResponseCreated"/> class.  
        /// </summary>  
        public ResponseCreated()
        {
            type = Type;
            response = new Response();
        }
    }
}