// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents an agent with associated metadata.
    /// </summary>
    public class Agent
    {
        /// <summary>
        ///     Gets or sets the type of the agent.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the name of the agent.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null;

        /// <summary>
        ///     Gets or sets the description of the agent.
        /// </summary>
        [JsonPropertyName("description")]
        public object Description { get; set; } = null;

        /// <summary>
        ///     Gets or sets the unique identifier of the agent.
        /// </summary>
        [JsonPropertyName("agent_id")]
        public string AgentId { get; set; } = null;

        /// <summary>
        ///     Gets or sets the thread identifier associated with the agent.
        /// </summary>
        [JsonPropertyName("thread_id")]
        public string ThreadId { get; set; } = null;
    }
}