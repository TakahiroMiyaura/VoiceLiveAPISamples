// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a response indicating that a content part has been added.
    /// </summary>
    public class ResponseContentPartAdded : MessageBase
    {
        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "response.content_part.added";

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
        ///     Gets or sets the part of the content.
        /// </summary>
        [JsonPropertyName("part")]
        public Part Part { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseContentPartAdded" /> class.
        /// </summary>
        public ResponseContentPartAdded()
        {
            Type = TypeName;
            ResponseId = string.Empty;
            ItemId = string.Empty;
            OutputIndex = 0;
            ContentIndex = 0;
            Part = new Part();
        }
    }
}