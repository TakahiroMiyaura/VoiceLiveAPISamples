// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that user speech has been detected.
    /// </summary>
    /// <remarks>
    ///     This class provides a unified representation of speech start detection.
    ///     It is the recommended replacement for the legacy <c>InputAudioBufferSpeechStarted</c> class.
    /// </remarks>
    public class SpeechStarted : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "input_audio_buffer.speech_started";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the timestamp in milliseconds when speech started.
        /// </summary>
        [JsonPropertyName("audio_start_ms")]
        public int AudioStartMs { get; set; }

        /// <summary>
        ///     Gets or sets the conversation item identifier associated with this speech.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpeechStarted" /> class.
        /// </summary>
        public SpeechStarted()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpeechStarted" /> class with the specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="audioStartMs">The audio start time in milliseconds.</param>
        /// <param name="itemId">The item identifier.</param>
        public SpeechStarted(string eventId, int audioStartMs, string itemId)
        {
            EventId = eventId;
            AudioStartMs = audioStartMs;
            ItemId = itemId;
        }

        #endregion
    }
}