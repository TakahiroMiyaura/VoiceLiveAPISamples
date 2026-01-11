// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the details of an error.
    /// </summary>
    public class ErrorDetail
    {
        /// <summary>
        ///     Gets or sets the error message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = null;

        /// <summary>
        ///     Gets or sets the type of the error.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the error code.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = null;

        /// <summary>
        ///     Gets or sets the parameter associated with the error.
        /// </summary>
        [JsonPropertyName("param")]
        public string Param { get; set; } = null;

        /// <summary>
        ///     Gets or sets the event ID related to the error.
        /// </summary>
        [JsonPropertyName("event_id")]
        public object EventId { get; set; } = null;
    }
}