// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a Foundry Agent tool definition for the Realtime API.
    /// </summary>
    /// <remarks>
    ///     This tool type enables calling Foundry Agent Service agents as tools
    ///     from the Voice Live real-time chat agent, implementing a "chat supervisor pattern".
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    public class FoundryAgentTool : RealtimeTool
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type discriminator value for Foundry Agent tools.
        /// </summary>
        public const string TypeDiscriminator = "foundry_agent";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the type of the tool. Always "foundry_agent" for Foundry Agent tools.
        /// </summary>
        [JsonPropertyName("type")]
        public override string Type { get; set; } = TypeDiscriminator;

        /// <summary>
        ///     Gets or sets the name of the Foundry agent to call.
        /// </summary>
        [JsonPropertyName("agent_name")]
        public string AgentName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the Foundry project containing the agent.
        /// </summary>
        [JsonPropertyName("project_name")]
        public string ProjectName { get; set; }

        /// <summary>
        ///     Gets or sets the version of the agent.
        /// </summary>
        [JsonPropertyName("agent_version")]
        public string AgentVersion { get; set; }

        /// <summary>
        ///     Gets or sets the client ID associated with the agent.
        /// </summary>
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        /// <summary>
        ///     Gets or sets the description of the tool.
        ///     When specified, this overrides the description from the Foundry portal.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the Foundry resource override for agent execution.
        /// </summary>
        [JsonPropertyName("foundry_resource_override")]
        public string FoundryResourceOverride { get; set; }

        /// <summary>
        ///     Gets or sets the agent context type.
        ///     Possible values: "no_context" (sends only current user input) or "agent_context" (agent maintains its own
        ///     context/thread).
        ///     Defaults to "agent_context".
        /// </summary>
        [JsonPropertyName("agent_context_type")]
        public string AgentContextType { get; set; }

        /// <summary>
        ///     Gets or sets whether the agent response should be returned directly.
        ///     When <c>true</c> (default), the agent's response is used as-is.
        ///     When <c>false</c>, the chat agent will paraphrase the response.
        /// </summary>
        [JsonPropertyName("return_agent_response_directly")]
        public bool? ReturnAgentResponseDirectly { get; set; }

        #endregion
    }
}
