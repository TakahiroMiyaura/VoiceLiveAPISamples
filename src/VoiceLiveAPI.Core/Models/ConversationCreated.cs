// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that a conversation has been created.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ConversationCreatedMessage</c> class.
    /// </remarks>
    public class ConversationCreated : ServerEvent
    {
        #region Properties

        /// <inheritdoc />
        public override string Type => "conversation.created";

        /// <summary>
        ///     Gets or sets the conversation identifier.
        /// </summary>
        [JsonPropertyName("conversation_id")]
        public string ConversationId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConversationCreated" /> class.
        /// </summary>
        public ConversationCreated()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConversationCreated" /> class with specified values.
        /// </summary>
        public ConversationCreated(string eventId, string conversationId)
        {
            EventId = eventId;
            ConversationId = conversationId;
        }

        #endregion
    }
}