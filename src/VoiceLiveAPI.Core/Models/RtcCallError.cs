// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;
using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Server event that reports an error for a failed WebRTC voice-session operation.
    /// </summary>
    /// <remarks>
    ///     Delivered as <c>rtc.call.error</c> on the control WebSocket. <see cref="ErrorDetail.Type" /> is
    ///     <c>invalid_request_error</c> for client errors or <c>server_error</c> for service-side failures.
    /// </remarks>
    public class RtcCallError : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "rtc.call.error";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the operation that failed (for example, <c>rtc.call.sdp.create</c>).
        /// </summary>
        [JsonPropertyName("operation")]
        public string Operation { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the WebRTC call/session, when available.
        /// </summary>
        [JsonPropertyName("rtc_call_id")]
        public string RtcCallId { get; set; }

        /// <summary>
        ///     Gets or sets the error details (<c>type</c>, <c>code</c>, <c>message</c>).
        /// </summary>
        [JsonPropertyName("error")]
        public ErrorDetail Error { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RtcCallError" /> class.
        /// </summary>
        public RtcCallError()
        {
        }

        #endregion
    }
}
