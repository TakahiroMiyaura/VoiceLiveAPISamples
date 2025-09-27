// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.Animations
{
    public class ResponseAnimationVisemeDelta : MessageBase
    {
        /// <summary>  
        /// The type identifier for the response audio delta message.  
        /// </summary>  
        public const string Type = "response.animation_viseme.delta";

        /// <summary>  
        /// Gets or sets the unique identifier for the response.  
        /// </summary>  
        public string response_id { get; set; }

        /// <summary>  
        /// Gets or sets the unique identifier for the item.  
        /// </summary>  
        public string item_id { get; set; }

        public int? output_index { get; set; }
        public int? content_index { get; set; }
        public int? audio_offset_ms { get; set; }
        public int? viseme_id { get; set; }
    }
}