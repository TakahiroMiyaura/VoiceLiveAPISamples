// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a response indicating that an output item has been added.
    /// </summary>
    public class ResponseOutputItemAdded : MessageBase
    {
        /// <summary>
        ///     The type identifier for this response.
        /// </summary>
        public const string TypeName = "response.output_item.added";

        /// <summary>
        ///     Gets or sets the response ID.
        /// </summary>

        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

        /// <summary>
        ///     Gets or sets the index of the output item.
        /// </summary>

        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the item associated with the response.
        /// </summary>

        [JsonPropertyName("item")]
        public Item Item { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseOutputItemAdded" /> class.
        /// </summary>
        public ResponseOutputItemAdded()
        {
            EventId = string.Empty;
            Type = TypeName;
            ResponseId = string.Empty;
            OutputIndex = 0;
            Item = new Item();
        }
    }
}