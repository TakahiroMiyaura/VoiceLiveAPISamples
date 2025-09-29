// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a response message containing viseme delta information for animation in VoiceLiveAPI.
    /// </summary>
    public class ResponseAnimationVisemeDelta : MessageBase
    {
        /// <summary>
        ///     The type identifier for the response audio delta message.
        /// </summary>
        public const string TypeName = "response.animation_viseme.delta";

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

        /// <summary>
        ///     Gets or sets the output index for the viseme delta in the animation response.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int? OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the content index for the viseme delta in the animation response.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int? ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the audio offset in milliseconds for the viseme delta in the animation response.
        /// </summary>
        [JsonPropertyName("audio_offset_ms")]
        public int? AudioOffsetMs { get; set; }

        /// <summary>
        ///     Gets or sets the viseme identifier for the viseme delta in the animation response.
        /// </summary>
        [JsonPropertyName("viseme_id")]
        public int? VisemeId { get; set; }
    }
}