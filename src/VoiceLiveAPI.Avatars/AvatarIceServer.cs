// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars
{
    /// <summary>
    ///     Dependency-neutral ICE server configuration for the avatar WebRTC connection.
    /// </summary>
    /// <remarks>
    ///     Decouples <see cref="AvatarClient" /> from any specific session SDK. Callers map the ICE
    ///     server information provided by the service (e.g. the official SDK's
    ///     <c>Session.Avatar.IceServers</c>, or the self-made Core's <c>IceServers</c>) onto this type.
    /// </remarks>
    public class AvatarIceServer
    {
        /// <summary>
        ///     Gets or sets the ICE server URLs.
        /// </summary>
        public string[] Urls { get; set; } = null;

        /// <summary>
        ///     Gets or sets the ICE server username.
        /// </summary>
        public string UserName { get; set; } = null;

        /// <summary>
        ///     Gets or sets the ICE server credential.
        /// </summary>
        public string Credential { get; set; } = null;
    }
}
