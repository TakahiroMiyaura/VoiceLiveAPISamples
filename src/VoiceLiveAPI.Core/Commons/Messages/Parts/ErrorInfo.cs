// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents error information.
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        ///     Gets or sets the error code.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the error message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the parameter that caused the error.
        /// </summary>
        [JsonPropertyName("param")]
        public string Param { get; set; } = null;

        /// <summary>
        ///     Gets or sets the event ID associated with the error.
        /// </summary>
        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = null;
    }
}