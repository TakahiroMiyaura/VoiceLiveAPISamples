// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message;
using System;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Sessions.Avatars
{
    /// <summary>
    /// Represents a session avatar connect message for initiating WebRTC avatar video streaming.
    /// </summary>
    public class SessionAvatarConnect : MessageBase
    {
        #region Static Fields and Constants

        /// <summary>
        /// The type of the message, indicating a session avatar connect.
        /// </summary>
        public const string Type = "session.avatar.connect";

        #endregion

        #region Public Fields

        /// <summary>
        /// Gets or sets the client-side Session Description Protocol (SDP) for WebRTC connection.
        /// </summary>
        public string client_sdp { get; set; } = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionAvatarConnect"/> class.
        /// </summary>
        public SessionAvatarConnect()
        {
            event_id = Guid.NewGuid().ToString();
            type = Type;
        }

        #endregion
    }
}