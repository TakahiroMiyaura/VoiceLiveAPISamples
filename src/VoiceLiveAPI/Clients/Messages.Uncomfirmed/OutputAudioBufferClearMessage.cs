// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Uncomfirmed
{

    /// <summary>
    ///     Represents an output audio buffer clear message.
    /// </summary>
    public class OutputAudioBufferClearMessage : VoiceLiveMessage
    {
        /// <summary>
        ///     The ID of the event.
        /// </summary>
        public string event_id { get; set; } = string.Empty;
    }
}