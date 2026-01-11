// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a server session that extends the client session with additional properties.
    /// </summary>
    public class ServerSession : ClientSession
    {
        /// <summary>
        ///     Gets or sets the instructions for the server session.
        /// </summary>
        [JsonPropertyName("instructions")]
        public string Instructions { get; set; } = null;

        /// <summary>
        ///     Gets or sets the tool choice for the server session.
        /// </summary>
        [JsonPropertyName("tool_choice")]
        public new string ToolChoice { get; set; } = null;

        /// <summary>
        ///     Gets or sets the temperature value for the server session.
        /// </summary>
        [JsonPropertyName("temperture")]
        public float? Temperture { get; set; }

        /// <summary>
        ///     Gets or sets the agent information for the server session.
        /// </summary>
        [JsonPropertyName("agent")]
        public Agent Agent { get; set; } = null;
    }
}