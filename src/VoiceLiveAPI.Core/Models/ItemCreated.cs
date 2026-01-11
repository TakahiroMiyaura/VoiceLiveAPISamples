// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that a conversation item has been created.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ConversationItemCreated</c> class.
    /// </remarks>
    public class ItemCreated : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "conversation.item.created";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the previous item identifier in the conversation.
        /// </summary>
        [JsonPropertyName("previous_item_id")]
        public string PreviousItemId { get; set; }

        /// <summary>
        ///     Gets or sets the created item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the item type.
        /// </summary>
        [JsonPropertyName("item_type")]
        public string ItemType { get; set; }

        /// <summary>
        ///     Gets or sets the item role (e.g., "user", "assistant").
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; }

        /// <summary>
        ///     Gets or sets the full item data.
        /// </summary>
        [JsonPropertyName("item")]
        public Item Item { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemCreated" /> class.
        /// </summary>
        public ItemCreated()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemCreated" /> class with specified values.
        /// </summary>
        public ItemCreated(string eventId, string previousItemId, string itemId, string itemType, string role)
        {
            EventId = eventId;
            PreviousItemId = previousItemId;
            ItemId = itemId;
            ItemType = itemType;
            Role = role;
        }

        #endregion
    }
}