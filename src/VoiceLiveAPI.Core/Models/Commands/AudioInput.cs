// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Commands
{
    /// <summary>
    ///     Represents an audio input command to send audio data to the session.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>InputAudioBufferAppend</c> class.
    /// </remarks>
    public class AudioInput
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The message type for this command.
        /// </summary>
        public const string TypeName = "input_audio_buffer.append";

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates an AudioInput from raw audio bytes.
        /// </summary>
        /// <param name="audioData">The raw audio data.</param>
        /// <returns>A new AudioInput instance.</returns>
        public static AudioInput FromBytes(byte[] audioData)
        {
            return new AudioInput(audioData);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the event identifier.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        ///     Gets or sets the base64-encoded audio data.
        /// </summary>
        public string Audio { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AudioInput" /> class.
        /// </summary>
        public AudioInput()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AudioInput" /> class with audio data.
        /// </summary>
        /// <param name="audioData">The raw audio data bytes.</param>
        public AudioInput(byte[] audioData)
        {
            Audio = audioData != null ? Convert.ToBase64String(audioData) : null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AudioInput" /> class with base64 audio.
        /// </summary>
        /// <param name="base64Audio">The base64-encoded audio data.</param>
        public AudioInput(string base64Audio)
        {
            Audio = base64Audio;
        }

        #endregion
    }
}