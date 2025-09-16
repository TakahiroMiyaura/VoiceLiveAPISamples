// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Conversations.Items
{

    /// <summary>  
    /// Represents a message indicating that a conversation item has been created.  
    /// </summary>  
    public class ConversationItemCreated : MessageBase
    {
        /// <summary>  
        /// The type identifier for the "conversation.item.created" message.  
        /// </summary>  
        public const string Type = "conversation.item.created";

        /// <summary>  
        /// Gets or sets the ID of the previous item in the conversation.  
        /// </summary>  
        public object previous_item_id { get; set; } = null;

        /// <summary>  
        /// Gets or sets the item associated with the conversation.  
        /// </summary>  
        public Item item { get; set; } = null;

        /// <summary>  
        /// Initializes a new instance of the <see cref="ConversationItemCreated"/> class.  
        /// </summary>  
        public ConversationItemCreated()
        {
            event_id = string.Empty;
            type = Type;
            previous_item_id = null;
            item = new Item();
        }
    }
}