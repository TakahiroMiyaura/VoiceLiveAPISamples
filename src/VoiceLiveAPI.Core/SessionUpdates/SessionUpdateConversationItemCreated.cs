// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Represents a session update indicating that a conversation item has been created.
    /// </summary>
    public class SessionUpdateConversationItemCreated : SessionUpdate
    {
        #region Constants

        /// <summary>
        ///     The type identifier for this session update.
        /// </summary>
        public const string TypeName = "conversation.item.created";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdateConversationItemCreated" /> class.
        /// </summary>
        /// <param name="message">The underlying message.</param>
        /// <param name="previousItemId">The previous item identifier.</param>
        /// <param name="item">The conversation item.</param>
        public SessionUpdateConversationItemCreated(MessageBase message, string previousItemId, Item item)
            : base(message)
        {
            PreviousItemId = previousItemId ?? string.Empty;
            Item = item;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the previous item identifier.
        /// </summary>
        public string PreviousItemId { get; }

        /// <summary>
        ///     Gets the conversation item.
        /// </summary>
        public Item Item { get; }

        #endregion
    }
}