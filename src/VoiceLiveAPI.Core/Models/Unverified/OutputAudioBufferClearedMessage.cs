// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Unverified
{
    /// <summary>
    ///     Represents an output audio buffer cleared message.
    /// </summary>
    public class OutputAudioBufferClearedMessage : MessageBase
    {
        /// <summary>
        ///     Gets or sets the response identifier.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; } = null;
    }
}