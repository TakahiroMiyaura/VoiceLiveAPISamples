// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Sessions.Avatars
{
    /// <summary>
    /// Represents a session avatar connecting message received from the server during WebRTC avatar setup.
    /// </summary>
    public class SessionAvatarConnecting : MessageBase
    {
        #region Static Fields and Constants

        /// <summary>
        /// The type of the message, indicating a session avatar is connecting.
        /// </summary>
        public const string Type = "session.avatar.connecting";

        #endregion

        #region Public Fields

        /// <summary>
        /// Gets or sets the server-side Session Description Protocol (SDP) for WebRTC connection.
        /// </summary>
        public string server_sdp { get; set; } = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionAvatarConnecting"/> class.
        /// </summary>
        public SessionAvatarConnecting()
        {
            event_id = Guid.NewGuid().ToString();
            type = Type;
        }

        #endregion
    }
}
