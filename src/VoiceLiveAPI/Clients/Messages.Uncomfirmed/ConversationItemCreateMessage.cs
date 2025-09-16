// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Uncomfirmed
{
    /// <summary>
    ///     Represents a conversation Item create message.
    /// </summary>
    public class ConversationItemCreateMessage : VoiceLiveMessage
    {
        /// <summary>
        ///     Gets or sets the previous Item ID.
        /// </summary>
        public string previous_item_id { get; set; } = null;

        /// <summary>
        ///     Gets or sets the Item to create.
        /// </summary>
        public ConversationRequestItem item { get; set; } = null;
    }

    /// <summary>  
    ///     Represents a request item in a conversation.  
    /// </summary>  
    public class ConversationRequestItem
    {
        /// <summary>  
        ///     The type of the Item.  
        ///     ÅEmessage  
        ///     ÅEfunction_call  
        ///     ÅEfunction_call_output  
        /// </summary>  
        public string type { get; set; } = null;

        /// <summary>  
        ///     The unique ID of the Item. The client can specify the ID to help manage server-side context. If the client doesn't  
        ///     provide an ID, the server generates one.  
        /// </summary>  
        public string id { get; set; } = null;
    }
}