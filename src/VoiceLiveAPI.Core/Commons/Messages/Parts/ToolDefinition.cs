// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Abstract base class for tool definitions in session configuration.
    /// </summary>
    /// <remarks>
    ///     The Voice Live API supports multiple tool types in the <c>tools</c> array:
    ///     <see cref="Function" /> (type: "function") and <see cref="McpToolConfig" /> (type: "mcp").
    /// </remarks>
    [JsonConverter(typeof(ToolDefinitionJsonConverter))]
    public abstract class ToolDefinition
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the type of the tool.
        /// </summary>
        [JsonPropertyName("type")]
        public abstract string Type { get; set; }

        #endregion
    }
}
