// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that a conversation item has been deleted.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ConversationItemDeletedMessage</c> class.
    /// </remarks>
    public class ItemDeleted : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "conversation.item.deleted";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the deleted item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemDeleted" /> class.
        /// </summary>
        public ItemDeleted()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemDeleted" /> class with specified values.
        /// </summary>
        public ItemDeleted(string eventId, string itemId)
        {
            EventId = eventId;
            ItemId = itemId;
        }

        #endregion
    }
}