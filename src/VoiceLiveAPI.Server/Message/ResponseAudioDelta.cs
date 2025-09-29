// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a response message containing audio delta information.
    /// </summary>
    public class ResponseAudioDelta : MessageBase
    {
        /// <summary>
        ///     The type identifier for the response audio delta message.
        /// </summary>
        public const string TypeName = "response.audio.delta";

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
        ///     Gets or sets the index of the output.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the index of the content.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the delta information for the audio.
        /// </summary>
        [JsonPropertyName("delta")]
        public string Delta { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseAudioDelta" /> class.
        /// </summary>
        public ResponseAudioDelta()
        {
            EventId = string.Empty;
            Type = TypeName;
            ResponseId = string.Empty;
            ItemId = string.Empty;
            OutputIndex = 0;
            ContentIndex = 0;
            Delta = string.Empty;
        }
    }
}