// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Messages
{
    /// <summary>
    ///     Represents a message for connecting a session avatar.
    /// </summary>
    [Obsolete(
        "This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages.SessionAvatarConnect instead.")]
    public class SessionAvatarConnect : ClientCommand
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type of the message, indicating a session avatar connect.
        /// </summary>
        public const string TypeName = "session.avatar.connect";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionAvatarConnect" /> class.
        /// </summary>
        public SessionAvatarConnect()
        {
            EventId = Guid.NewGuid().ToString();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     SDP information from the client.
        /// </summary>
        [JsonPropertyName("client_sdp")]
        public string ClientSdp { get; set; } = string.Empty;

        #endregion
    }
}