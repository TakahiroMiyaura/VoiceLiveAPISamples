// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents an error from the VoiceLive service.
    /// </summary>
    /// <remarks>
    ///     This class provides a unified representation of error information.
    /// </remarks>
    public class VoiceLiveError : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "error";

        #endregion

        #region Public Methods

        /// <summary>
        ///     Returns a string representation of this error.
        /// </summary>
        /// <returns>A string containing the error code and message.</returns>
        public override string ToString()
        {
            return $"[{Code}] {Message}";
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the error code.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        ///     Gets or sets the error message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        ///     Gets or sets the error type category (e.g., "invalid_request_error").
        /// </summary>
        [JsonPropertyName("error_type")]
        public string ErrorType { get; set; }

        /// <summary>
        ///     Gets or sets the parameter that caused the error, if applicable.
        /// </summary>
        [JsonPropertyName("param")]
        public string Param { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveError" /> class.
        /// </summary>
        public VoiceLiveError()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveError" /> class with the specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="type">The error type.</param>
        /// <param name="param">The parameter that caused the error.</param>
        public VoiceLiveError(string eventId, string code, string message, string errorType = null, string param = null)
        {
            EventId = eventId;
            Code = code;
            Message = message;
            ErrorType = errorType;
            Param = param;
        }

        #endregion
    }
}