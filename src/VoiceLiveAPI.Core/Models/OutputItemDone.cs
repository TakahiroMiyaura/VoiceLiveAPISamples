// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that an output item has been completed.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ResponseOutputItemDone</c> class.
    /// </remarks>
    public class OutputItemDone : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "response.output_item.done";

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

        /// <summary>
        ///     Gets or sets the full item data.
        /// </summary>
        [JsonPropertyName("item")]
        public Item Item { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="OutputItemDone" /> class.
        /// </summary>
        public OutputItemDone()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OutputItemDone" /> class with specified values.
        /// </summary>
        public OutputItemDone(string eventId, string responseId, int outputIndex, string itemId)
        {
            EventId = eventId;
            ResponseId = responseId;
            OutputIndex = outputIndex;
            ItemId = itemId;
        }

        #endregion
    }
}