// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.Animations
{

    /// <summary>  
    /// Represents a response message containing animation viseme delta information.  
    /// </summary>  
    public class ResponseAnimationVisemeDelta : MessageBase
    {
        /// <summary>  
        /// The type identifier for the response animation viseme delta message.  
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

        /// <summary>  
        /// Gets or sets the index of the output.  
        /// </summary>  
        public int? output_index { get; set; }

        /// <summary>  
        /// Gets or sets the index of the content.  
        /// </summary>  
        public int? content_index { get; set; }

        /// <summary>  
        /// Gets or sets the audio offset in milliseconds.  
        /// </summary>  
        public int? audio_offset_ms { get; set; }

        /// <summary>  
        /// Gets or sets the viseme identifier.  
        /// </summary>  
        public int? viseme_id { get; set; }

        /// <summary>  
        /// Initializes a new instance of the <see cref="ResponseAnimationVisemeDelta"/> class.  
        /// </summary>  
        public ResponseAnimationVisemeDelta()
        {
            event_id = string.Empty;
            type = Type;
            response_id = string.Empty;
            item_id = string.Empty;
            output_index = null;
            content_index = null;
            audio_offset_ms = null;
            viseme_id = null;
        }
    }
}