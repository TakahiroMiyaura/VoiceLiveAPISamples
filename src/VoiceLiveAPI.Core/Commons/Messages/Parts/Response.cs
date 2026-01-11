// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the response from the VoiceLive API.
    /// </summary>
    public class Response
    {
        /// <summary>
        ///     Gets or sets the unique identifier of the response.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null;

        /// <summary>
        ///     Gets or sets the status of the response.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = null;

        /// <summary>
        ///     Gets or sets the details of the response status.
        /// </summary>
        [JsonPropertyName("status_details")]
        public StatusDetails StatusDetails { get; set; } = null;

        /// <summary>
        ///     Gets or sets the output data of the response.
        /// </summary>
        [JsonPropertyName("output")]
        public object[] Output { get; set; } = null;

        /// <summary>
        ///     Gets or sets the usage details of the response.
        /// </summary>
        [JsonPropertyName("usage")]
        public Usage Usage { get; set; } = null;
    }
}