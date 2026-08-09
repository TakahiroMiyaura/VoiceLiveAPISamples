// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Abstract base class for tool call status events (in_progress, completed, failed).
    /// </summary>
    /// <remarks>
    ///     Provides shared properties for tool call lifecycle status events.
    ///     Used by <see cref="McpCallInProgress" />, <see cref="McpCallCompleted" />, and <see cref="McpCallFailed" />.
    /// </remarks>
    public abstract class ToolCallStatus : ServerEvent
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the output index.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the response the invoked agent produced (Foundry agent calls
        ///     only; null for MCP and while the call is still starting).
        /// </summary>
        /// <remarks>
        ///     This is the only handle the session gives you on what the agent actually did. It matters when
        ///     the agent runs on a Model Router deployment: neither this session nor tracing reveals which
        ///     model the router picked — you have to read the response itself
        ///     (<c>{project endpoint}/openai/v1/responses/{id}</c>), whose <c>model</c> field names it.
        /// </remarks>
        [JsonPropertyName("agent_response_id")]
        public string AgentResponseId { get; set; }

        #endregion
    }
}
