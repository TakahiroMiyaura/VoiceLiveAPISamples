// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages.Unverified
{
    /// <summary>
    ///     Represents a conversation Item create message.
    /// </summary>
    public class ConversationItemCreateMessage : ClientCommand
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "conversation.item.create";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

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

        #endregion
    }

    /// <summary>
    ///     Represents a request item in a conversation.
    /// </summary>
    public class ConversationRequestItem
    {
        /// <summary>
        ///     The type of the Item.
        ///     �Emessage
        ///     �Efunction_call
        ///     �Efunction_call_output
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