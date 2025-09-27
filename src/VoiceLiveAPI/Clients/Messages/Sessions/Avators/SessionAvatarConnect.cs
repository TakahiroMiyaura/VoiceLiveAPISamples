// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message;
using System;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Sessions.Avatars
{
    public class SessionAvatarConnect : MessageBase
    {
        /// <summary>
        ///     The type of the message, indicating a session avatar connect.
        /// </summary>
        public const string Type = "session.avatar.connect";

        public string client_sdp { get; set; } = string.Empty;

        public SessionAvatarConnect()
        {
            event_id = Guid.NewGuid().ToString();
            type = Type;
        }
    }
}