// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents an MCP list tools failed event.
    /// </summary>
    /// <remarks>
    ///     Received when the service fails to list available tools from an MCP server.
    ///     Available in API version 2025-10-01 and later.
    /// </remarks>
    public class McpListToolsFailed : McpListToolsStatus
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type name for this event.
        /// </summary>
        public const string TypeName = "mcp_list_tools.failed";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        #endregion
    }
}
