// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;
using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Server event carrying the SDP answer after a WebRTC voice session is created successfully,
    ///     in response to <see cref="Commands.Messages.RtcCallSdpCreate" />.
    /// </summary>
    /// <remarks>
    ///     Delivered as <c>rtc.call.sdp.created</c> on the control WebSocket. Apply
    ///     <see cref="SdpAnswer" /> as the remote description to complete the WebRTC negotiation.
    /// </remarks>
    public class RtcCallSdpCreated : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "rtc.call.sdp.created";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the SDP answer from the service for WebRTC negotiation.
        /// </summary>
        [JsonPropertyName("sdp_answer")]
        public string SdpAnswer { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the WebRTC call/session.
        /// </summary>
        [JsonPropertyName("rtc_call_id")]
        public string RtcCallId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RtcCallSdpCreated" /> class.
        /// </summary>
        public RtcCallSdpCreated()
        {
        }

        #endregion
    }
}
