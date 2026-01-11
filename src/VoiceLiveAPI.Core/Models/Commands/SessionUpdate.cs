// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Commands
{
    /// <summary>
    ///     Represents a command to update session configuration.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ClientSessionUpdated</c> class.
    /// </remarks>
    public class SessionUpdate
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The message type for this command.
        /// </summary>
        public const string TypeName = "session.update";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the event identifier.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        ///     Gets or sets the session configuration.
        /// </summary>
        public object Session { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdate" /> class.
        /// </summary>
        public SessionUpdate()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdate" /> class with session config.
        /// </summary>
        public SessionUpdate(object session)
        {
            Session = session;
        }

        #endregion
    }
}