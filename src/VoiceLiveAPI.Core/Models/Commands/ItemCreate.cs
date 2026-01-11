// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Commands
{
    /// <summary>
    ///     Represents a command to create a conversation item.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ConversationItemCreateMessage</c> class.
    /// </remarks>
    public class ItemCreate
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The message type for this command.
        /// </summary>
        public const string TypeName = "conversation.item.create";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemCreate" /> class.
        /// </summary>
        public ItemCreate()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the event identifier.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        ///     Gets or sets the previous item identifier.
        /// </summary>
        public string PreviousItemId { get; set; }

        /// <summary>
        ///     Gets or sets the item type.
        /// </summary>
        public string ItemType { get; set; }

        /// <summary>
        ///     Gets or sets the item role.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        ///     Gets or sets the item content.
        /// </summary>
        public object Content { get; set; }

        #endregion
    }
}