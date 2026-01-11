// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Unverified.Messages
{
    /// <summary>
    ///     Represents a conversation Item truncate message.
    /// </summary>
    [Obsolete(
        "This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages.Unverified.ConversationItemTruncateMessage instead.")]
    public class ConversationItemTruncateMessage : ClientCommand
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "conversation.item.truncate";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     The ID of the assistant message Item to truncate. Only assistant message items can be truncated.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        ///     The index of the content part to truncate. Set this property to "0".
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Inclusive duration up to which audio is truncated, in milliseconds. If the audio_end_ms is greater than the actual
        ///     audio duration, the server responds with an error.
        /// </summary>
        [JsonPropertyName("audio_end_ms")]
        public int? AudioEndMs { get; set; }

        #endregion
    }
}