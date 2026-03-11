// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the base configuration for filler responses.
    /// </summary>
    /// <remarks>
    ///     Filler responses are used to avoid silence during wait times or tool call execution.
    ///     Derived types include <see cref="BasicFillerResponseConfig" /> (type: "static_filler") and
    ///     <see cref="LlmFillerResponseConfig" /> (type: "llm_filler").
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    [JsonConverter(typeof(FillerResponseConfigJsonConverter))]
    public abstract class FillerResponseConfig
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the type of the filler response.
        ///     Possible values: "static_filler" or "llm_filler".
        /// </summary>
        [JsonPropertyName("type")]
        public abstract string Type { get; set; }

        /// <summary>
        ///     Gets or sets the trigger conditions for the filler response.
        ///     Possible values: "latency" (triggered when response wait time exceeds threshold)
        ///     and "tool" (triggered during tool call execution).
        ///     Defaults to ["latency"].
        /// </summary>
        [JsonPropertyName("triggers")]
        public string[] Triggers { get; set; }

        /// <summary>
        ///     Gets or sets the latency threshold in milliseconds before triggering the filler response.
        ///     Defaults to 2000.
        /// </summary>
        [JsonPropertyName("latency_threshold_ms")]
        public int? LatencyThresholdMs { get; set; }

        #endregion
    }
}
