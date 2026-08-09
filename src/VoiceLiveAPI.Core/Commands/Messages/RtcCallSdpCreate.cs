// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages
{
    /// <summary>
    ///     Client event that initiates a Voice Live WebRTC voice session by sending an SDP offer over the
    ///     control WebSocket (endpoint <c>/voice-live/realtime/calls</c>). The service replies with
    ///     <see cref="Models.RtcCallSdpCreated" /> (SDP answer) or <see cref="Models.RtcCallError" />.
    /// </summary>
    /// <remarks>
    ///     This is the signaling for the WebRTC <b>voice</b> transport (audio flows over RTP media tracks),
    ///     which is distinct from the WebRTC <b>avatar</b> path (<see cref="SessionAvatarConnect" />).
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    public class RtcCallSdpCreate : ClientCommand
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this command.
        /// </summary>
        public const string TypeName = "rtc.call.sdp.create";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the SDP offer from the client for WebRTC negotiation.
        /// </summary>
        [JsonPropertyName("sdp_offer")]
        public string SdpOffer { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the optional initial session configuration. If provided, it is applied before the
        ///     session is established (same shape as the <c>session.update</c> payload). Typed as
        ///     <see cref="object" /> so a <see cref="VoiceLiveSessionOptions" /> can be passed directly and
        ///     serialized by its runtime type, matching how <c>session.update</c> is sent.
        /// </summary>
        [JsonPropertyName("session")]
        public object Session { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RtcCallSdpCreate" /> class.
        /// </summary>
        public RtcCallSdpCreate()
        {
            EventId = Guid.NewGuid().ToString();
        }

        #endregion
    }
}
