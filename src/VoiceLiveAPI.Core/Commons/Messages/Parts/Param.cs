// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a parameter with a type and description.
    /// </summary>
    public class Param
    {
        /// <summary>
        ///     Gets or sets the type of the parameter.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the description of the parameter.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = null;
    }
}