// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a message indicating that an input audio buffer has been committed.
    /// </summary>
    public class InputAudioBufferCommitted : MessageBase
    {
        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "input_audio_buffer.committed";

        /// <summary>
        ///     Gets or sets the identifier of the previous item in the buffer.
        /// </summary>
        [JsonPropertyName("previous_item_id")]
        public object PreviousItemId { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the current item in the buffer.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputAudioBufferCommitted" /> class.
        /// </summary>
        public InputAudioBufferCommitted()
        {
            EventId = Guid.NewGuid().ToString();
            Type = TypeName;
            PreviousItemId = string.Empty;
            ItemId = string.Empty;
        }
    }
}