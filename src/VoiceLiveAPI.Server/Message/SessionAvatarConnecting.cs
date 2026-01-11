// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars.Servers.Message
{
    /// <summary>
    ///     Represents a message indicating that a session avatar is connecting.
    /// </summary>
    public class SessionAvatarConnecting : MessageBase
    {
        /// <summary>
        ///     The type of the message, indicating a session avatar is connecting.
        /// </summary>
        public const string TypeName = "session.avatar.connecting";

        /// <summary>
        ///     The SDP information from the server.
        /// </summary>
        [JsonPropertyName("server_sdp")]
        public string ServerSdp { get; set; } = null;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionAvatarConnecting" /> class.
        /// </summary>
        public SessionAvatarConnecting()
        {
            EventId = Guid.NewGuid().ToString();
            Type = TypeName;
        }
    }
}