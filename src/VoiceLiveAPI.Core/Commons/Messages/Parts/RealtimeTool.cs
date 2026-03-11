// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the base class for all tool definitions in the Realtime API.
    /// </summary>
    /// <remarks>
    ///     Derived types include <see cref="Function" /> (type: "function") and
    ///     <see cref="FoundryAgentTool" /> (type: "foundry_agent").
    /// </remarks>
    [JsonConverter(typeof(RealtimeToolJsonConverter))]
    public abstract class RealtimeTool
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
