// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Commands
{
    /// <summary>
    ///     Represents a command to connect an avatar session.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>SessionAvatarConnect</c> class.
    /// </remarks>
    public class AvatarConnect
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The message type for this command.
        /// </summary>
        public const string TypeName = "session.avatar.connect";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the event identifier.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        ///     Gets or sets the SDP offer for WebRTC connection.
        /// </summary>
        public string SdpOffer { get; set; }

        /// <summary>
        ///     Gets or sets the avatar configuration.
        /// </summary>
        public object Avatar { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AvatarConnect" /> class.
        /// </summary>
        public AvatarConnect()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AvatarConnect" /> class with SDP offer.
        /// </summary>
        public AvatarConnect(string sdpOffer, object avatar = null)
        {
            SdpOffer = sdpOffer;
            Avatar = avatar;
        }

        #endregion
    }
}