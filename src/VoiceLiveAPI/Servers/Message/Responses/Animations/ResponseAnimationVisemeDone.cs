// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.Animations
{

    /// <summary>  
    /// Represents a response message indicating animation viseme completion.  
    /// </summary>  
    public class ResponseAnimationVisemeDone : MessageBase
    {
        /// <summary>  
        /// The type identifier for the response animation viseme done message.  
        /// </summary>  
        public const string Type = "response.animation_viseme.done";

        /// <summary>  
        /// Gets or sets the unique identifier for the response.  
        /// </summary>  
        public string response_id { get; set; }

        /// <summary>  
        /// Gets or sets the unique identifier for the item.  
        /// </summary>  
        public string item_id { get; set; }

        /// <summary>  
        /// Initializes a new instance of the <see cref="ResponseAnimationVisemeDone"/> class.  
        /// </summary>  
        public ResponseAnimationVisemeDone()
        {
            event_id = string.Empty;
            type = Type;
            response_id = string.Empty;
            item_id = string.Empty;
        }
    }
}
