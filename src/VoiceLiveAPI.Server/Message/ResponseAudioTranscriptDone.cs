// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents the response indicating that an audio transcription process has completed.
    /// </summary>
    public class ResponseAudioTranscriptDone : MessageBase
    {
        /// <summary>
        ///     The type identifier for this response.
        /// </summary>
        public const string TypeName = "response.audio_transcript.done";

        /// <summary>
        ///     Gets or sets the unique identifier for the response.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

        /// <summary>
        ///     Gets or sets the unique identifier for the item being transcribed.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the index of the output in the transcription process.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the index of the content in the transcription process.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the transcript text generated from the audio.
        /// </summary>
        [JsonPropertyName("transcript")]
        public string Transcript { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseAudioTranscriptDone" /> class.
        /// </summary>
        public ResponseAudioTranscriptDone()
        {
            EventId = string.Empty;
            Type = TypeName;
            ResponseId = string.Empty;
            ItemId = string.Empty;
            OutputIndex = 0;
            ContentIndex = 0;
            Transcript = string.Empty;
        }
    }
}