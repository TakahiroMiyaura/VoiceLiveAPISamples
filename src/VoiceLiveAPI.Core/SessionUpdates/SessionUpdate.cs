// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Represents the base class for all session updates in the VoiceLive API.
    ///     This follows the official Azure.AI.VoiceLive SDK pattern for event processing.
    /// </summary>
    public abstract class SessionUpdate
    {
        #region Public Methods

        /// <summary>
        ///     Creates a SessionUpdate from a raw message.
        /// </summary>
        /// <param name="message">The raw message.</param>
        /// <returns>A SessionUpdate instance, or null if the message type is not recognized.</returns>
        public static SessionUpdate FromMessage(MessageBase message)
        {
            return SessionUpdateFactory.Create(message);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the unique identifier for this event.
        /// </summary>
        public string EventId { get; }

        /// <summary>
        ///     Gets the type of the session update.
        /// </summary>
        public string Type { get; }

        /// <summary>
        ///     Gets the underlying raw message that this update wraps.
        /// </summary>
        public MessageBase RawMessage { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdate" /> class.
        /// </summary>
        /// <param name="message">The underlying message.</param>
        protected SessionUpdate(MessageBase message)
        {
            RawMessage = message;
            EventId = message?.EventId ?? string.Empty;
            Type = message?.Type ?? string.Empty;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdate" /> class with specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="type">The event type.</param>
        protected SessionUpdate(string eventId, string type)
        {
            RawMessage = null;
            EventId = eventId ?? string.Empty;
            Type = type ?? string.Empty;
        }

        #endregion
    }
}