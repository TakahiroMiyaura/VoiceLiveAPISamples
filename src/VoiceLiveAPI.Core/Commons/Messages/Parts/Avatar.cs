// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents an avatar configuration (character, style, customization, ICE servers, video,
    ///     scene and output settings) sent as the session <c>avatar</c> object.
    /// </summary>
    /// <remarks>
    ///     Field parity with the official <c>Azure.AI.VoiceLive.AvatarConfiguration</c>. Available in
    ///     API version 2025-10-01 and later. Use <see cref="Types" />, <see cref="OutputProtocols" />
    ///     and <see cref="PhotoBaseModes" /> for the allowed string values.
    /// </remarks>
    public class Avatar
    {
        /// <summary>
        ///     Allowed values for <see cref="Type" /> (avatar kind).
        /// </summary>
        public static class Types
        {
            /// <summary>A video-based avatar.</summary>
            public const string VideoAvatar = "video-avatar";

            /// <summary>A photo-based avatar.</summary>
            public const string PhotoAvatar = "photo-avatar";
        }

        /// <summary>
        ///     Allowed values for <see cref="OutputProtocol" />.
        /// </summary>
        public static class OutputProtocols
        {
            /// <summary>WebRTC media output.</summary>
            public const string WebRtc = "webrtc";

            /// <summary>WebSocket media output.</summary>
            public const string WebSocket = "websocket";
        }

        /// <summary>
        ///     Allowed values for <see cref="Model" /> (photo avatar base model).
        /// </summary>
        public static class PhotoBaseModes
        {
            /// <summary>The VASA-1 photo avatar base model.</summary>
            public const string Vasa1 = "vasa-1";
        }

        /// <summary>
        ///     Gets or sets the avatar kind ("video-avatar" or "photo-avatar"). See <see cref="Types" />.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the character of the avatar.
        /// </summary>
        [JsonPropertyName("character")]
        public string Character { get; set; } = null;

        /// <summary>
        ///     Gets or sets the style of the avatar.
        /// </summary>
        [JsonPropertyName("style")]
        public string Style { get; set; } = null;

        /// <summary>
        ///     Gets or sets a value indicating whether the avatar is customized.
        /// </summary>
        [JsonPropertyName("customized")]
        public bool? Customized { get; set; }

        /// <summary>
        ///     Gets or sets the ICE servers associated with the avatar.
        /// </summary>
        [JsonPropertyName("ice_servers")]
        public IceServers[] IceServers { get; set; } = null;

        /// <summary>
        ///     Gets or sets the photo avatar base model (e.g. "vasa-1"). See <see cref="PhotoBaseModes" />.
        ///     Applicable when <see cref="Type" /> is "photo-avatar".
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = null;

        /// <summary>
        ///     Gets or sets the video configuration for the avatar.
        /// </summary>
        [JsonPropertyName("video")]
        public Video Video { get; set; } = null;

        /// <summary>
        ///     Gets or sets the scene parameters (zoom, position, rotation, amplitude) for the avatar.
        /// </summary>
        [JsonPropertyName("scene")]
        public Scene Scene { get; set; } = null;

        /// <summary>
        ///     Gets or sets the media output protocol ("webrtc" or "websocket"). See <see cref="OutputProtocols" />.
        /// </summary>
        [JsonPropertyName("output_protocol")]
        public string OutputProtocol { get; set; } = null;

        /// <summary>
        ///     Gets or sets a value indicating whether to output audit audio.
        /// </summary>
        [JsonPropertyName("output_audit_audio")]
        public bool? OutputAuditAudio { get; set; }
    }
}