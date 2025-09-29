// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a delta response for audio transcript processing.
    /// </summary>
    public class ResponseAudioTranscriptDelta : MessageBase
    {
        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "response.audio_transcript.delta";

        /// <summary>
        ///     Gets or sets the unique identifier for the response.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

        /// <summary>
        ///     Gets or sets the unique identifier for the item.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the index of the output.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the index of the content.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the delta content of the transcript.
        /// </summary>
        [JsonPropertyName("delta")]
        public string Delta { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseAudioTranscriptDelta" /> class.
        /// </summary>
        public ResponseAudioTranscriptDelta()
        {
            EventId = string.Empty;
            Type = TypeName;
            ResponseId = string.Empty;
            ItemId = string.Empty;
            OutputIndex = 0;
            ContentIndex = 0;
            Delta = string.Empty;
        }
    }
}