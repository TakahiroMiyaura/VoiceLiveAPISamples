// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{

    /// <summary>
    ///     Represents a base message from the VoiceInfo Live API.
    /// </summary>
    public class VoiceLiveMessage
    {
        /// <summary>
        ///     Gets or sets the message type.
        /// </summary>
        public string type { get; set; } = string.Empty;
    }
}