using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.Animations
{
    public class ResponseAnimationVisemeDone : MessageBase
    {
        /// <summary>  
        /// The type identifier for the response audio delta message.  
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
    }
}
