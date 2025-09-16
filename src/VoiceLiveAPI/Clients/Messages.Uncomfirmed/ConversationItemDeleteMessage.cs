// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Uncomfirmed
{
    /// <summary>
    ///     Represents a conversation Item delete message.
    /// </summary>
    public class ConversationItemDeleteMessage : VoiceLiveMessage
    {
        /// <summary>
        ///     Gets or sets the Item ID to delete.
        /// </summary>
        public string item_id { get; set; } = string.Empty;
    }
}