// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Represents a session update indicating that a session has been created.
    /// </summary>
    public class SessionUpdateSessionCreated : SessionUpdate
    {
        #region Constants

        /// <summary>
        ///     The type identifier for this session update.
        /// </summary>
        public const string TypeName = "session.created";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the session information.
        /// </summary>
        public ServerSession Session { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdateSessionCreated" /> class.
        /// </summary>
        /// <param name="message">The underlying message.</param>
        /// <param name="session">The session information.</param>
        public SessionUpdateSessionCreated(MessageBase message, ServerSession session) : base(message)
        {
            Session = session;
        }

        #endregion
    }
}