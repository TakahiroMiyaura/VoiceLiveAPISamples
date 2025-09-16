// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses
{

    /// <summary>  
    /// Represents a response message indicating that a process has completed.  
    /// </summary>  
    public class ResponseDone : MessageBase
    {
        /// <summary>  
        /// The type identifier for the response.done message.  
        /// </summary>  
        public const string Type = "response.done";

        /// <summary>  
        /// Gets or sets the response details associated with the completion.  
        /// </summary>  
        public Response response { get; set; }

        /// <summary>  
        /// Initializes a new instance of the <see cref="ResponseDone"/> class.  
        /// </summary>  
        public ResponseDone()
        {
            type = Type;
            response = new Response();
        }
    }
}