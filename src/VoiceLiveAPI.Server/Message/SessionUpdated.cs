// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents an updated server session message in the VoiceLiveAPI.
    /// </summary>
    public class ServerSessionUpdated : MessageBase
    {
        /// <summary>
        ///     The type identifier for the updated session message.
        /// </summary>
        public const string TypeName = "session.updated";

        /// <summary>
        ///     Gets or sets the server session associated with the update.
        /// </summary>

        [JsonPropertyName("session")]
        public ServerSession Session { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServerSessionUpdated" /> class.
        /// </summary>
        public ServerSessionUpdated()
        {
            Type = TypeName;
            Session = new ServerSession();
        }
    }
}