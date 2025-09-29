// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a message indicating that a session has been created.
    /// </summary>
    public class SessionCreated : MessageBase
    {
        /// <summary>
        ///     The type of the message.
        /// </summary>
        public const string TypeName = "session.created";

        /// <summary>
        ///     The session information associated with the created session.
        /// </summary>
        [JsonPropertyName("session")]
        public ServerSession Session { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionCreated" /> class.
        /// </summary>
        public SessionCreated()
        {
            EventId = Guid.NewGuid().ToString();
            Type = TypeName;
            Session = new ServerSession();
        }
    }
}