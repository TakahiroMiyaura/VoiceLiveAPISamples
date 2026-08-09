// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents an MCP approval request item received from the server.
    /// </summary>
    /// <remarks>
    ///     Sent when <c>require_approval</c> is set to "always" on an MCP tool.
    ///     The client must respond with an <c>mcp_approval_response</c> conversation item
    ///     to approve or deny the tool execution.
    ///     Available in API version 2025-10-01 and later.
    /// </remarks>
    public class McpApprovalRequest : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type name for this server event.
        /// </summary>
        public const string TypeName = "mcp_approval_request";

        /// <summary>
        ///     The type name for the client response conversation item.
        /// </summary>
        public const string ResponseTypeName = "mcp_approval_response";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the label of the MCP server.
        /// </summary>
        [JsonPropertyName("server_label")]
        public string ServerLabel { get; set; }

        /// <summary>
        ///     Gets or sets the name of the tool to call.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the arguments for the MCP call.
        /// </summary>
        [JsonPropertyName("arguments")]
        public string Arguments { get; set; }

        #endregion
    }
}
