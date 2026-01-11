// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events
{
    /// <summary>
    ///     Base class for all server-to-client events in the VoiceLive API.
    /// </summary>
    /// <remarks>
    ///     This class represents events received from the server through the WebSocket connection.
    ///     All model classes in <c>Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models</c> should inherit from this class.
    /// </remarks>
    public abstract class ServerEvent
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the unique identifier for this event.
        /// </summary>
        [JsonPropertyName("event_id")]
        public string EventId { get; set; }

        /// <summary>
        ///     Gets the type identifier for this event.
        /// </summary>
        /// <remarks>
        ///     Each derived class should return its specific event type string
        ///     (e.g., "response.audio.delta", "session.created").
        /// </remarks>
        [JsonPropertyName("type")]
        public abstract string Type { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServerEvent" /> class.
        /// </summary>
        protected ServerEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServerEvent" /> class with the specified event ID.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        protected ServerEvent(string eventId)
        {
            EventId = eventId;
        }

        #endregion
    }
}