// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>
    /// Represents information about updated rate limits.
    /// </summary>
    public class LimitsUpdatedRateLimitsItembjectInfo
    {
        /// <summary>
        /// Gets or sets the rate limit type.
        /// </summary>
        public string name { get; set; } = null;

        /// <summary>
        /// Gets or sets the rate limit value.
        /// </summary>
        public int limit { get; set; }

        /// <summary>
        /// Gets or sets the remaining rate limit value.
        /// </summary>
        public int remaining { get; set; }

        /// <summary>
        /// Gets or sets the rate limit reset time in seconds.
        /// </summary>
        public int reset_seconds { get; set; }
    }
}