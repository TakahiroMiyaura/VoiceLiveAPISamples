// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed
{

    /// <summary>
    ///     Represents a rate limits updated message.
    /// </summary>
    public class RateLimitsUpdatedMessage : VoiceLiveMessage
    {
        /// <summary>
        ///     Gets or sets the rate limits information.
        /// </summary>
        public LimitsUpdatedRateLimitsItembjectInfo rate_limits { get; set; } = null;
    }
}