// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified
{
    /// <summary>
    ///     Represents information about updated rate limits.
    /// </summary>
    public class LimitsUpdatedRateLimitsItemObjectInfo
    {
        /// <summary>
        ///     Gets or sets the rate limit type.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null;

        /// <summary>
        ///     Gets or sets the rate limit value.
        /// </summary>
        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        /// <summary>
        ///     Gets or sets the remaining rate limit value.
        /// </summary>
        [JsonPropertyName("remaining")]
        public int Remaining { get; set; }

        /// <summary>
        ///     Gets or sets the rate limit reset time in seconds.
        /// </summary>
        [JsonPropertyName("reset_seconds")]
        public int ResetSeconds { get; set; }
    }
}