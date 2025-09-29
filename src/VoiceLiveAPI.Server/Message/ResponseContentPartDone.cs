// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a response indicating that a content part has been completed.
    /// </summary>
    public class ResponseContentPartDone : MessageBase
    {
        /// <summary>
        ///     The type identifier for this response.
        /// </summary>
        public const string TypeName = "response.content_part.done";

        /// <summary>
        ///     Gets or sets the response ID.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

        /// <summary>
        ///     Gets or sets the item ID.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the output index.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the content index.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the part information.
        /// </summary>
        [JsonPropertyName("part")]
        public Part Part { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseContentPartDone" /> class.
        /// </summary>
        public ResponseContentPartDone()
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