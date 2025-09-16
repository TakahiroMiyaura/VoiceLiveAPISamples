// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Uncomfirmed
{
    /// <summary>
    ///     Represents a conversation Item truncate message.
    /// </summary>
    public class ConversationItemTruncateMessage : VoiceLiveMessage
    {
        /// <summary>
        ///     The ID of the assistant message Item to truncate. Only assistant message items can be truncated.
        /// </summary>
        public string item_id { get; set; } = string.Empty;

        /// <summary>
        ///     The index of the content part to truncate. Set this property to "0".
        /// </summary>
        public int content_index { get; set; }

        /// <summary>
        ///     Inclusive duration up to which audio is truncated, in milliseconds. If the audio_end_ms is greater than the actual
        ///     audio duration, the server responds with an error.
        /// </summary>
        public int? audio_end_ms { get; set; }
    }
}