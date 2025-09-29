// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages
{
    /// <summary>
    ///     Represents the base class for messages in the VoiceLiveAPI.
    /// </summary>
    public class MessageBase
    {
        /// <summary>
        ///     Gets or sets the unique identifier for the event.
        /// </summary>
        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = null;

        /// <summary>
        ///     Gets or sets the type of the message.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;
    }
}