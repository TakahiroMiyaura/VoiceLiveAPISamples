// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that a conversation item has been retrieved.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ConversationItemRetrievedMessage</c> class.
    /// </remarks>
    public class ItemRetrieved : ServerEvent
    {
        #region Properties

        /// <inheritdoc />
        public override string Type => "conversation.item.retrieved";

        /// <summary>
        ///     Gets or sets the retrieved item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemRetrieved" /> class.
        /// </summary>
        public ItemRetrieved()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemRetrieved" /> class with specified values.
        /// </summary>
        public ItemRetrieved(string eventId, string itemId)
        {
            EventId = eventId;
            ItemId = itemId;
        }

        #endregion
    }
}