// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Represents a session update indicating that speech has started in the input audio buffer.
    /// </summary>
    public class SessionUpdateInputAudioBufferSpeechStarted : SessionUpdate
    {
        #region Constants

        /// <summary>
        ///     The type identifier for this session update.
        /// </summary>
        public const string TypeName = "input_audio_buffer.speech_started";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdateInputAudioBufferSpeechStarted" /> class.
        /// </summary>
        /// <param name="message">The underlying message.</param>
        /// <param name="audioStartMs">The audio start time in milliseconds.</param>
        /// <param name="itemId">The item identifier.</param>
        public SessionUpdateInputAudioBufferSpeechStarted(MessageBase message, int audioStartMs, string itemId)
            : base(message)
        {
            AudioStartMs = audioStartMs;
            ItemId = itemId ?? string.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the audio start time in milliseconds.
        /// </summary>
        public int AudioStartMs { get; }

        /// <summary>
        ///     Gets the item identifier.
        /// </summary>
        public string ItemId { get; }

        #endregion
    }
}