// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a Foundry agent tool definition for session configuration.
    /// </summary>
    /// <remarks>
    ///     Exposes a Foundry agent as a tool of the current session, enabling the "chat-supervisor" pattern: a
    ///     realtime chat model handles the basic back-and-forth and delegates complex work to a more capable
    ///     Foundry agent. The service invokes the agent itself — the client never receives a
    ///     <c>function_call</c>; it observes progress through the <c>response.foundry_agent_call.*</c> events.
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    public class FoundryAgentToolConfig : ToolDefinition
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the type of the tool. Always "foundry_agent" for Foundry agent tools.
        /// </summary>
        [JsonPropertyName("type")]
        public override string Type { get; set; } = "foundry_agent";

        /// <summary>
        ///     Gets or sets the name of the Foundry agent to call. Required.
        /// </summary>
        [JsonPropertyName("agent_name")]
        public string AgentName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the Foundry project containing the agent. Required.
        /// </summary>
        [JsonPropertyName("project_name")]
        public string ProjectName { get; set; }

        /// <summary>
        ///     Gets or sets the version of the Foundry agent to call. Optional; the latest version is used when
        ///     omitted.
        /// </summary>
        [JsonPropertyName("agent_version")]
        public string AgentVersion { get; set; }

        /// <summary>
        ///     Gets or sets the client ID associated with the Foundry agent. Optional.
        /// </summary>
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        /// <summary>
        ///     Gets or sets a description of the tool. Optional; when provided it is used instead of the
        ///     agent's description from the Foundry portal, so it is what the model reasons about when
        ///     deciding to delegate.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets an override for the Foundry resource used to execute the agent. Optional; needed
        ///     when the agent lives on a different resource than the Voice Live session.
        /// </summary>
        [JsonPropertyName("foundry_resource_override")]
        public string FoundryResourceOverride { get; set; }

        /// <summary>
        ///     Gets or sets the context type used when invoking the agent: <c>agent_context</c> (default — the
        ///     agent keeps its own thread and only the current input is sent per call) or <c>no_context</c>
        ///     (only the current user input is sent, no context is maintained). Optional.
        /// </summary>
        [JsonPropertyName("agent_context_type")]
        public string AgentContextType { get; set; }

        /// <summary>
        ///     Gets or sets whether the agent's response is returned directly in the Voice Live response
        ///     (default <c>true</c>). When <c>false</c>, the response is handed to the chat model to rephrase.
        ///     Optional.
        /// </summary>
        [JsonPropertyName("return_agent_response_directly")]
        public bool? ReturnAgentResponseDirectly { get; set; }

        #endregion
    }
}
