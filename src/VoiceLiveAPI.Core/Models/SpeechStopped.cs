// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that user speech has ended.
    /// </summary>
    /// <remarks>
    ///     This class provides a unified representation of speech end detection.
    ///     It is the recommended replacement for the legacy <c>InputAudioBufferSpeechStopped</c> class.
    /// </remarks>
    public class SpeechStopped : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "input_audio_buffer.speech_stopped";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the timestamp in milliseconds when speech ended.
        /// </summary>
        [JsonPropertyName("audio_end_ms")]
        public int AudioEndMs { get; set; }

        /// <summary>
        ///     Gets or sets the conversation item identifier associated with this speech.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpeechStopped" /> class.
        /// </summary>
        public SpeechStopped()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpeechStopped" /> class with the specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="audioEndMs">The audio end time in milliseconds.</param>
        /// <param name="itemId">The item identifier.</param>
        public SpeechStopped(string eventId, int audioEndMs, string itemId)
        {
            EventId = eventId;
            AudioEndMs = audioEndMs;
            ItemId = itemId;
        }

        #endregion
    }
}