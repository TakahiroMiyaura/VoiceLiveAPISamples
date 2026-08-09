// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Abstract base class for tool call arguments delta events.
    /// </summary>
    /// <remarks>
    ///     Provides shared properties for streaming tool call arguments.
    ///     Used by <see cref="McpCallArgumentsDelta" />.
    /// </remarks>
    public abstract class ToolCallArgumentsDelta : ServerEvent
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the response identifier.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

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
        ///     Gets or sets the arguments delta as a JSON string.
        /// </summary>
        [JsonPropertyName("delta")]
        public string Delta { get; set; }

        #endregion
    }
}
