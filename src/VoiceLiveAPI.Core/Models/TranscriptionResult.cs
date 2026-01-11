// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents the completed transcription of user input audio.
    /// </summary>
    /// <remarks>
    ///     This class provides a unified representation of completed transcription data.
    ///     It is the recommended replacement for the legacy
    ///     <c>ConversationItemInputAudioTranscriptionCompleted</c> class.
    /// </remarks>
    public class TranscriptionResult : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "conversation.item.input_audio_transcription.completed";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the conversation item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the content index within the item.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the complete transcript text.
        /// </summary>
        [JsonPropertyName("transcript")]
        public string Transcript { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TranscriptionResult" /> class.
        /// </summary>
        public TranscriptionResult()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TranscriptionResult" /> class with the specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="contentIndex">The content index.</param>
        /// <param name="transcript">The complete transcript text.</param>
        public TranscriptionResult(string eventId, string itemId, int contentIndex, string transcript)
        {
            EventId = eventId;
            ItemId = itemId;
            ContentIndex = contentIndex;
            Transcript = transcript;
        }

        #endregion
    }
}