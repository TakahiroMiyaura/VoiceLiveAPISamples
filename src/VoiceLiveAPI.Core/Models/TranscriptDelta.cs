// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a transcript delta containing streaming transcript text from the AI response.
    /// </summary>
    /// <remarks>
    ///     This class provides a unified representation of transcript streaming data.
    ///     It is the recommended replacement for the legacy <c>ResponseAudioTranscriptDelta</c> class.
    /// </remarks>
    public class TranscriptDelta : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "response.audio_transcript.delta";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the response identifier.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

        /// <summary>
        ///     Gets or sets the item identifier within the response.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the output index in the response.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the content index within the output.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the transcript text delta.
        /// </summary>
        [JsonPropertyName("delta")]
        public string Delta { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TranscriptDelta" /> class.
        /// </summary>
        public TranscriptDelta()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TranscriptDelta" /> class with the specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        /// <param name="contentIndex">The content index.</param>
        /// <param name="delta">The transcript text delta.</param>
        public TranscriptDelta(string eventId, string responseId, string itemId, int outputIndex, int contentIndex,
            string delta)
        {
            EventId = eventId;
            ResponseId = responseId;
            ItemId = itemId;
            OutputIndex = outputIndex;
            ContentIndex = contentIndex;
            Delta = delta;
        }

        #endregion
    }
}