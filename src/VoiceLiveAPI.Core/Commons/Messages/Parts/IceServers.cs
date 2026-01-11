// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the configuration for ICE (Interactive Connectivity Establishment) servers.
    /// </summary>
    public class IceServers
    {
        /// <summary>
        ///     Gets or sets the URLs of the ICE servers.
        /// </summary>
        [JsonPropertyName("urls")]
        public string[] Urls { get; set; } = null;

        /// <summary>
        ///     Gets or sets the username for the ICE server authentication.
        /// </summary>
        [JsonPropertyName("username")]
        public string UserName { get; set; } = null;

        /// <summary>
        ///     Gets or sets the credential for the ICE server authentication.
        /// </summary>
        [JsonPropertyName("credential")]
        public string Credential { get; set; } = null;
    }
}