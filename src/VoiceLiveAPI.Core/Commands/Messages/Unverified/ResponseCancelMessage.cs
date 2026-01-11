// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages.Unverified
{
    /// <summary>
    ///     Represents a response cancel message.
    /// </summary>
    public class ResponseCancelMessage : ClientCommand
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "response.cancel";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        #endregion
    }
}