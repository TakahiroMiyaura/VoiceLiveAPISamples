// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Commands
{
    /// <summary>
    ///     Represents a command to truncate a conversation item.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ConversationItemTruncateMessage</c> class.
    /// </remarks>
    public class ItemTruncate
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The message type for this command.
        /// </summary>
        public const string TypeName = "conversation.item.truncate";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the event identifier.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        ///     Gets or sets the item identifier to truncate.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the content index to truncate at.
        /// </summary>
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the audio end time in milliseconds.
        /// </summary>
        public int AudioEndMs { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemTruncate" /> class.
        /// </summary>
        public ItemTruncate()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemTruncate" /> class with specified values.
        /// </summary>
        public ItemTruncate(string itemId, int contentIndex, int audioEndMs)
        {
            ItemId = itemId;
            ContentIndex = contentIndex;
            AudioEndMs = audioEndMs;
        }

        #endregion
    }
}