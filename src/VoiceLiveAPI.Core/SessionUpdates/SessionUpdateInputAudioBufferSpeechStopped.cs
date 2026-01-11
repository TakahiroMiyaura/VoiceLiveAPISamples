// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Represents a session update indicating that speech has stopped in the input audio buffer.
    /// </summary>
    public class SessionUpdateInputAudioBufferSpeechStopped : SessionUpdate
    {
        #region Constants

        /// <summary>
        ///     The type identifier for this session update.
        /// </summary>
        public const string TypeName = "input_audio_buffer.speech_stopped";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdateInputAudioBufferSpeechStopped" /> class.
        /// </summary>
        /// <param name="message">The underlying message.</param>
        /// <param name="audioEndMs">The audio end time in milliseconds.</param>
        /// <param name="itemId">The item identifier.</param>
        public SessionUpdateInputAudioBufferSpeechStopped(MessageBase message, int audioEndMs, string itemId)
            : base(message)
        {
            AudioEndMs = audioEndMs;
            ItemId = itemId ?? string.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the audio end time in milliseconds.
        /// </summary>
        public int AudioEndMs { get; }

        /// <summary>
        ///     Gets the item identifier.
        /// </summary>
        public string ItemId { get; }

        #endregion
    }
}