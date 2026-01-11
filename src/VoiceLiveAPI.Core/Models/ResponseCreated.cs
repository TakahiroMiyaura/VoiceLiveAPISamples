// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that a new response has been created.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ResponseCreated</c> class.
    /// </remarks>
    public class ResponseCreated : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "response.created";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the response identifier.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

        /// <summary>
        ///     Gets or sets the response status.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseCreated" /> class.
        /// </summary>
        public ResponseCreated()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseCreated" /> class with specified values.
        /// </summary>
        public ResponseCreated(string eventId, string responseId, string status)
        {
            EventId = eventId;
            ResponseId = responseId;
            Status = status;
        }

        #endregion
    }
}