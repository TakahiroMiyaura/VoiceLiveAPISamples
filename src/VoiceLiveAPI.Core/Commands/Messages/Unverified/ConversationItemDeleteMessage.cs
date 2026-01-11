// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages.Unverified
{
    /// <summary>
    ///     Represents a conversation Item delete message.
    /// </summary>
    public class ConversationItemDeleteMessage : ClientCommand
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "conversation.item.delete";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the Item ID to delete.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; } = string.Empty;

        #endregion
    }
}