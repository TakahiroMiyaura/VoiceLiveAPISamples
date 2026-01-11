// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Messages
{
    /// <summary>
    ///     Represents a conversation created message.
    /// </summary>
    public class ConversationCreatedMessage : MessageBase
    {
        /// <summary>
        ///     Gets or sets the conversation object.
        /// </summary>
        [JsonPropertyName("conversation")]
        public ConversationObjectInfo Conversation { get; set; } = null;
    }
}