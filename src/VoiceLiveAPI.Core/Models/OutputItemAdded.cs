// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that an output item has been added to a response.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ResponseOutputItemAdded</c> class.
    /// </remarks>
    public class OutputItemAdded : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "response.output_item.added";

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
        ///     Gets or sets the output index.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="OutputItemAdded" /> class.
        /// </summary>
        public OutputItemAdded()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OutputItemAdded" /> class with specified values.
        /// </summary>
        public OutputItemAdded(string eventId, string responseId, int outputIndex, string itemId)
        {
            EventId = eventId;
            ResponseId = responseId;
            OutputIndex = outputIndex;
            ItemId = itemId;
        }

        #endregion
    }
}