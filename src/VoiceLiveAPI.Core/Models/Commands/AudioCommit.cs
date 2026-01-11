// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Commands
{
    /// <summary>
    ///     Represents a command to commit the input audio buffer.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>InputAudioBufferCommitMessage</c> class.
    /// </remarks>
    public class AudioCommit
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The message type for this command.
        /// </summary>
        public const string TypeName = "input_audio_buffer.commit";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the event identifier.
        /// </summary>
        public string EventId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AudioCommit" /> class.
        /// </summary>
        public AudioCommit()
        {
        }

        #endregion
    }
}