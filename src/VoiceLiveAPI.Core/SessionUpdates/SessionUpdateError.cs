// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Represents a session update containing an error.
    /// </summary>
    public class SessionUpdateError : SessionUpdate
    {
        #region Constants

        /// <summary>
        ///     The type identifier for this session update.
        /// </summary>
        public const string TypeName = "error";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdateError" /> class.
        /// </summary>
        /// <param name="message">The underlying message.</param>
        /// <param name="code">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="errorType">The error type.</param>
        public SessionUpdateError(MessageBase message, string code, string errorMessage, string errorType)
            : base(message)
        {
            Code = code ?? string.Empty;
            Message = errorMessage ?? string.Empty;
            ErrorType = errorType ?? string.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the error code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        ///     Gets the error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///     Gets the error type.
        /// </summary>
        public string ErrorType { get; }

        #endregion
    }
}