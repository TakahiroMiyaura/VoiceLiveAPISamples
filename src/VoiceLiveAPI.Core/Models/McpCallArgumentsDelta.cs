// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a streaming MCP call arguments delta event.
    /// </summary>
    /// <remarks>
    ///     Received when the model-generated MCP tool call arguments are updated.
    ///     Available in API version 2025-10-01 and later.
    /// </remarks>
    public class McpCallArgumentsDelta : ToolCallArgumentsDelta
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type name for this event.
        /// </summary>
        public const string TypeName = "response.mcp_call_arguments.delta";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        #endregion
    }
}
