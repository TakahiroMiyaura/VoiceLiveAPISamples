// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that output audio playback has started.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>OutputAudioBufferStartedMessage</c> class.
    /// </remarks>
    public class OutputAudioStarted : ServerEvent
    {
        #region Properties

        /// <inheritdoc />
        public override string Type => "output_audio_buffer.started";

        /// <summary>
        ///     Gets or sets the response identifier.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="OutputAudioStarted" /> class.
        /// </summary>
        public OutputAudioStarted()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OutputAudioStarted" /> class with specified values.
        /// </summary>
        public OutputAudioStarted(string eventId, string responseId)
        {
            EventId = eventId;
            ResponseId = responseId;
        }

        #endregion
    }
}