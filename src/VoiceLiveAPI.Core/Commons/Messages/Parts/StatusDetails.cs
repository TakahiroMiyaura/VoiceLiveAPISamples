// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the details of a status, including its type and reason.
    /// </summary>
    public class StatusDetails
    {
        /// <summary>
        ///     Gets or sets the type of the status.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the reason for the status.
        /// </summary>
        [JsonPropertyName("reason")]
        public string Reason { get; set; } = null;
    }
}