// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a message indicating that the viseme animation has completed in the response.
    /// </summary>
    public class ResponseAnimationVisemeDone : MessageBase
    {
        /// <summary>
        ///     The type identifier for the response audio delta message.
        /// </summary>
        public const string TypeName = "response.animation_viseme.done";

        /// <summary>
        ///     Gets or sets the unique identifier for the response.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

        /// <summary>
        ///     Gets or sets the unique identifier for the item.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }
    }
}