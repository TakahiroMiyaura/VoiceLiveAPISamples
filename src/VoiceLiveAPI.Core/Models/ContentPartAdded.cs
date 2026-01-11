// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that a content part has been added.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ResponseContentPartAdded</c> class.
    /// </remarks>
    public class ContentPartAdded : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "response.content_part.added";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the response identifier.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

        /// <summary>
        ///     Gets or sets the item identifier.
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
        ///     Gets or sets the content part type.
        /// </summary>
        [JsonPropertyName("part_type")]
        public string PartType { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentPartAdded" /> class.
        /// </summary>
        public ContentPartAdded()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentPartAdded" /> class with specified values.
        /// </summary>
        public ContentPartAdded(string eventId, string responseId, string itemId, int outputIndex, int contentIndex,
            string partType)
        {
            EventId = eventId;
            ResponseId = responseId;
            ItemId = itemId;
            OutputIndex = outputIndex;
            ContentIndex = contentIndex;
            PartType = partType;
        }

        #endregion
    }
}