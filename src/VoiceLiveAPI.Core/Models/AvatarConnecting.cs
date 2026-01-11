// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that an avatar session is connecting.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>SessionAvatarConnecting</c> class.
    /// </remarks>
    public class AvatarConnecting : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "session.avatar.connecting";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the SDP answer for WebRTC connection.
        /// </summary>
        [JsonPropertyName("server_sdp")]
        public string SdpAnswer { get; set; }

        /// <summary>
        ///     Gets the server SDP (alias for <see cref="SdpAnswer" />).
        /// </summary>
        [JsonIgnore]
        public string ServerSdp => SdpAnswer;

        /// <summary>
        ///     Gets or sets the ICE servers configuration.
        /// </summary>
        [JsonPropertyName("ice_servers")]
        public object IceServers { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AvatarConnecting" /> class.
        /// </summary>
        public AvatarConnecting()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AvatarConnecting" /> class with specified values.
        /// </summary>
        public AvatarConnecting(string eventId, string sdpAnswer, object iceServers)
        {
            EventId = eventId;
            SdpAnswer = sdpAnswer;
            IceServers = iceServers;
        }

        #endregion
    }
}