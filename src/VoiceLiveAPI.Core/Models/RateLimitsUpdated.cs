// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that rate limits have been updated.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>RateLimitsUpdatedMessage</c> class.
    /// </remarks>
    public class RateLimitsUpdated : ServerEvent
    {
        #region Properties

        /// <inheritdoc />
        public override string Type => "rate_limits.updated";

        /// <summary>
        ///     Gets or sets the rate limits information.
        /// </summary>
        [JsonPropertyName("rate_limits")]
        public IList<RateLimitInfo> RateLimits { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RateLimitsUpdated" /> class.
        /// </summary>
        public RateLimitsUpdated()
        {
            RateLimits = new List<RateLimitInfo>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RateLimitsUpdated" /> class with specified values.
        /// </summary>
        public RateLimitsUpdated(string eventId, IList<RateLimitInfo> rateLimits)
        {
            EventId = eventId;
            RateLimits = rateLimits ?? new List<RateLimitInfo>();
        }

        #endregion
    }

    /// <summary>
    ///     Represents rate limit information for a specific resource.
    /// </summary>
    public class RateLimitInfo
    {
        /// <summary>
        ///     Gets or sets the rate limit name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the maximum limit.
        /// </summary>
        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        /// <summary>
        ///     Gets or sets the remaining quota.
        /// </summary>
        [JsonPropertyName("remaining")]
        public int Remaining { get; set; }

        /// <summary>
        ///     Gets or sets the reset time in seconds.
        /// </summary>
        [JsonPropertyName("reset_seconds")]
        public double ResetSeconds { get; set; }
    }
}