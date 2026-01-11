// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that audio transcription has failed.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy
    ///     <c>ConversationItemInputAudioTranscriptionFailedMessage</c> class.
    /// </remarks>
    public class TranscriptionFailed : ServerEvent
    {
        #region Properties

        /// <inheritdoc />
        public override string Type => "conversation.item.input_audio_transcription.failed";

        /// <summary>
        ///     Gets or sets the item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the content index.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the error information.
        /// </summary>
        [JsonPropertyName("error")]
        public VoiceLiveError Error { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TranscriptionFailed" /> class.
        /// </summary>
        public TranscriptionFailed()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TranscriptionFailed" /> class with specified values.
        /// </summary>
        public TranscriptionFailed(string eventId, string itemId, int contentIndex, VoiceLiveError error)
        {
            EventId = eventId;
            ItemId = itemId;
            ContentIndex = contentIndex;
            Error = error;
        }

        #endregion
    }
}