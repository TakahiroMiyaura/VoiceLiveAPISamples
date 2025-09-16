// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Uncomfirmed
{
    /// <summary>
    ///     Represents a conversation Item retrieve message.
    /// </summary>
    public class ConversationItemRetrieveMessage : VoiceLiveMessage
    {
        /// <summary>
        ///     The ID of the Item to retrieve.
        /// </summary>
        public string item_id { get; set; } = string.Empty;


        /// <summary>
        ///     The ID of the event.
        /// </summary>
        public string event_id { get; set; } = string.Empty;
    }
}