using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Sessions.Avatars
{
    public class SessionAvatarConnecting : MessageBase
    {

        /// <summary>
        ///     The type of the message, indicating a session avatar is connecting.
        /// </summary>
        public const string Type = "session.avatar.connecting";

        public string server_sdp { get; set; } = null;

        public SessionAvatarConnecting()
        {
            event_id = Guid.NewGuid().ToString();
            type = Type;
        }
    }
}
