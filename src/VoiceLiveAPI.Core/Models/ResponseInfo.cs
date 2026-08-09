// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents response information from the VoiceLive service.
    /// </summary>
    /// <remarks>
    ///     This class provides a unified representation of response completion data.
    ///     It is the recommended replacement for the legacy <c>ResponseDone</c> class.
    /// </remarks>
    public class ResponseInfo : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "response.done";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the response identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the object type.
        /// </summary>
        [JsonPropertyName("object")]
        public string Object { get; set; }

        /// <summary>
        ///     Gets or sets the response status (e.g., "completed", "cancelled", "failed").
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        ///     Gets or sets additional status details.
        /// </summary>
        [JsonPropertyName("status_details")]
        public object StatusDetails { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the conversation this response belongs to.
        /// </summary>
        /// <remarks>
        ///     Populated in agent sessions, where it is the Foundry conversation (<c>conv_…</c>) the agent kept
        ///     the turn in. It is the only handle an agent session gives you on the agent's own execution:
        ///     listing responses (<c>{project endpoint}/openai/v1/responses</c>) and matching on each one's
        ///     <c>conversation.id</c> yields the response that answered, including the model — which is how you
        ///     find out what a Model Router deployment actually picked.
        /// </remarks>
        [JsonPropertyName("conversation_id")]
        public string ConversationId { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the response completed successfully.
        /// </summary>
        public bool IsCompleted => Status == "completed";

        /// <summary>
        ///     Gets a value indicating whether the response was cancelled.
        /// </summary>
        public bool IsCancelled => Status == "cancelled";

        /// <summary>
        ///     Gets a value indicating whether the response failed.
        /// </summary>
        public bool IsFailed => Status == "failed";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseInfo" /> class.
        /// </summary>
        public ResponseInfo()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseInfo" /> class with the specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="id">The response identifier.</param>
        /// <param name="status">The response status.</param>
        public ResponseInfo(string eventId, string id, string status)
        {
            EventId = eventId;
            Id = id;
            Status = status;
        }

        #endregion
    }
}