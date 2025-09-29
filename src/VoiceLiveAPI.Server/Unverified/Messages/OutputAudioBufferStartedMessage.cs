// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Messages
{
    /// <summary>
    ///     Represents an output audio buffer started message.
    /// </summary>
    public class OutputAudioBufferStartedMessage : MessageBase
    {
        /// <summary>
        ///     Gets or sets the response ID associated with the message.
        /// </summary>
        public string response_id = null;
    }
}