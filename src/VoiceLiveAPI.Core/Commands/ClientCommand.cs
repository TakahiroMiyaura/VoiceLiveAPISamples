// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands
{
    /// <summary>
    ///     Base class for all client-to-server commands in the VoiceLive API.
    /// </summary>
    /// <remarks>
    ///     This class represents commands sent from the client to the server through the WebSocket connection.
    ///     All command classes in <c>Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Commands</c> should inherit from this class.
    /// </remarks>
    public abstract class ClientCommand
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the optional event identifier for this command.
        /// </summary>
        /// <remarks>
        ///     The event ID is optional for client commands but can be used for tracking purposes.
        /// </remarks>
        [JsonPropertyName("event_id")]
        public string EventId { get; set; }

        /// <summary>
        ///     Gets the type identifier for this command.
        /// </summary>
        /// <remarks>
        ///     Each derived class should return its specific command type string
        ///     (e.g., "input_audio_buffer.append", "session.update").
        /// </remarks>
        [JsonPropertyName("type")]
        public abstract string Type { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientCommand" /> class.
        /// </summary>
        protected ClientCommand()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientCommand" /> class with the specified event ID.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        protected ClientCommand(string eventId)
        {
            EventId = eventId;
        }

        #endregion
    }
}