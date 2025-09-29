// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents an avatar with properties for character, style, customization, ICE servers, and video configuration.
    /// </summary>
    public class Avatar
    {
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
        ///     Gets or sets the video configuration for the avatar.
        /// </summary>
        [JsonPropertyName("video")]
        public Video Video { get; set; } = null;
    }
}