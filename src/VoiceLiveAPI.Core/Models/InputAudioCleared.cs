// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that the input audio buffer has been cleared.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>InputAudioBufferClearedMessage</c> class.
    /// </remarks>
    public class InputAudioCleared : ServerEvent
    {
        #region Properties

        /// <inheritdoc />
        public override string Type => "input_audio_buffer.cleared";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputAudioCleared" /> class.
        /// </summary>
        public InputAudioCleared()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InputAudioCleared" /> class with specified values.
        /// </summary>
        public InputAudioCleared(string eventId)
        {
            EventId = eventId;
        }

        #endregion
    }
}