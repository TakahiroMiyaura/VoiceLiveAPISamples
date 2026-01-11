// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Unverified.Messages
{
    /// <summary>
    ///     Represents an input audio buffer commit message.
    /// </summary>
    [Obsolete(
        "This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages.Unverified.InputAudioBufferCommitMessage instead.")]
    public class InputAudioBufferCommitMessage : ClientCommand
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "input_audio_buffer.commit";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        #endregion
    }
}