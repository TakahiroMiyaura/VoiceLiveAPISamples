// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a message indicating that a conversation item has been created.
    /// </summary>
    public class ConversationItemCreated : MessageBase
    {
        /// <summary>
        ///     The type identifier for the "conversation.item.created" message.
        /// </summary>
        public const string TypeName = "conversation.item.created";

        /// <summary>
        ///     Gets or sets the ID of the previous item in the conversation.
        /// </summary>
        [JsonPropertyName("previous_item_id")]
        public object PreviousItemId { get; set; }

        /// <summary>
        ///     Gets or sets the item associated with the conversation.
        /// </summary>
        [JsonPropertyName("item")]
        public Item Item { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConversationItemCreated" /> class.
        /// </summary>
        public ConversationItemCreated()
        {
            EventId = string.Empty;
            Type = TypeName;
            PreviousItemId = null;
            Item = new Item();
        }
    }
}