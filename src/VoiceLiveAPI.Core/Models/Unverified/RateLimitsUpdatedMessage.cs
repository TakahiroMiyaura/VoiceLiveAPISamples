// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Unverified
{
    /// <summary>
    ///     Represents a rate limits updated message.
    /// </summary>
    public class RateLimitsUpdatedMessage : MessageBase
    {
        /// <summary>
        ///     Gets or sets the rate limits information.
        /// </summary>
        [JsonPropertyName("rate_limits")]
        public LimitsUpdatedRateLimitsItemObjectInfo RateLimits { get; set; } = null;
    }
}