// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Sessions
{
    /// <summary>
    /// Represents an updated server session message in the VoiceLiveAPI.
    /// This message is sent by the server to notify clients of session updates.
    /// </summary>
    public class ServerSessionUpdated : MessageBase
    {
        #region Static Fields

        /// <summary>
        /// The type identifier for the updated session message.
        /// </summary>
        public const string Type = "session.updated";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the server session associated with the update.
        /// Contains the current state of the session after the update.
        /// </summary>
        /// <value>
        /// A <see cref="ServerSession"/> object representing the updated session state.
        /// </value>
        public ServerSession session { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSessionUpdated"/> class.
        /// Sets the message type and initializes an empty session object.
        /// </summary>
        public ServerSessionUpdated()
        {
            type = Type;
            session = new ServerSession();
        }

        #endregion
    }
}
