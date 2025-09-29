// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Unverified.Messages
{
    /// <summary>
    ///     Represents a conversation Item create message.
    /// </summary>
    public class ConversationItemCreateMessage : MessageBase
    {
        /// <summary>
        ///     Gets or sets the previous Item ID.
        /// </summary>
        [JsonPropertyName("previous_item_id")]
        public string PreviousItemId { get; set; } = null;

        /// <summary>
        ///     Gets or sets the Item to create.
        /// </summary>
        [JsonPropertyName("item")]
        public ConversationRequestItem Item { get; set; } = null;
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
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     The unique ID of the Item. The client can specify the ID to help manage server-side context. If the client doesn't
        ///     provide an ID, the server generates one.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null;
    }
}