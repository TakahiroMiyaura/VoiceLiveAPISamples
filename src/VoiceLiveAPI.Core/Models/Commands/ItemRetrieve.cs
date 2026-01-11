// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Commands
{
    /// <summary>
    ///     Represents a command to retrieve a conversation item.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ConversationItemRetrieveMessage</c> class.
    /// </remarks>
    public class ItemRetrieve
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The message type for this command.
        /// </summary>
        public const string TypeName = "conversation.item.retrieve";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the event identifier.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        ///     Gets or sets the item identifier to retrieve.
        /// </summary>
        public string ItemId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemRetrieve" /> class.
        /// </summary>
        public ItemRetrieve()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemRetrieve" /> class with specified item.
        /// </summary>
        public ItemRetrieve(string itemId)
        {
            ItemId = itemId;
        }

        #endregion
    }
}