// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Messages
{
    /// <summary>
    ///     Represents a conversation Item retrieved message.
    /// </summary>
    public class ConversationItemRetrievedMessage : MessageBase
    {
        /// <summary>
        ///     Gets or sets the information about the retrieved conversation item.
        /// </summary>
        public ConversationResponseItemInfo ItemInfo { get; set; } = null;
    }
}