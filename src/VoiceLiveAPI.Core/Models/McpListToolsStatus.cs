// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Abstract base class for MCP list tools status events.
    /// </summary>
    /// <remarks>
    ///     Provides shared properties for MCP tool listing lifecycle events.
    ///     Used by <see cref="McpListToolsInProgress" />, <see cref="McpListToolsCompleted" />,
    ///     and <see cref="McpListToolsFailed" />.
    /// </remarks>
    public abstract class McpListToolsStatus : ServerEvent
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the MCP list tools item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        #endregion
    }
}
