// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents an MCP server tool definition for session configuration.
    /// </summary>
    /// <remarks>
    ///     Configures a remote MCP server whose tools become available to the AI model.
    ///     The API service automatically manages tool calls on behalf of the client.
    ///     Available in API version 2025-10-01 and later.
    /// </remarks>
    public class McpToolConfig : ToolDefinition
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the type of the tool. Always "mcp" for MCP tools.
        /// </summary>
        [JsonPropertyName("type")]
        public override string Type { get; set; } = "mcp";

        /// <summary>
        ///     Gets or sets the label of the MCP server.
        /// </summary>
        [JsonPropertyName("server_label")]
        public string ServerLabel { get; set; }

        /// <summary>
        ///     Gets or sets the URL of the remote MCP server.
        /// </summary>
        [JsonPropertyName("server_url")]
        public string ServerUrl { get; set; }

        /// <summary>
        ///     Gets or sets the list of allowed tool names.
        ///     If not specified, all tools are allowed.
        /// </summary>
        [JsonPropertyName("allowed_tools")]
        public string[] AllowedTools { get; set; }

        /// <summary>
        ///     Gets or sets additional headers to include in MCP requests.
        /// </summary>
        [JsonPropertyName("headers")]
        public JsonElement? Headers { get; set; }

        /// <summary>
        ///     Gets or sets the authorization token for MCP requests.
        /// </summary>
        [JsonPropertyName("authorization")]
        public string Authorization { get; set; }

        /// <summary>
        ///     Gets or sets the approval requirement for MCP tool execution.
        /// </summary>
        /// <remarks>
        ///     Can be a string ("never" or "always") or a dictionary with per-tool settings.
        ///     Default value is "always". When set to "always", an <c>mcp_approval_request</c>
        ///     is sent to the client before execution. Set to "never" for automatic execution.
        /// </remarks>
        [JsonPropertyName("require_approval")]
        public JsonElement? RequireApproval { get; set; }

        #endregion
    }
}
