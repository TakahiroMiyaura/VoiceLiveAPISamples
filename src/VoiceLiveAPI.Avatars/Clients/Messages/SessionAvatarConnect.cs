// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars.Clients.Messages
{
    /// <summary>
    ///     Represents a message for connecting a session avatar.
    /// </summary>
    [Obsolete(
        "This class is obsolete. Please use the Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages.SessionAvatarConnect",
        true)]
    public class SessionAvatarConnect : MessageBase
    {
        /// <summary>
        ///     The type of the message, indicating a session avatar connect.
        /// </summary>
        public const string TypeName = "session.avatar.connect";

        /// <summary>
        ///     SDP information from the client.
        /// </summary>
        [JsonPropertyName("client_sdp")]
        public string ClientSdp { get; set; } = string.Empty;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionAvatarConnect" /> class.
        /// </summary>
        public SessionAvatarConnect()
        {
            EventId = Guid.NewGuid().ToString();
            Type = TypeName;
        }
    }
}