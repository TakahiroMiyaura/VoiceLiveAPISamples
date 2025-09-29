// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a set of parameters with their types, descriptions, and required fields.
    /// </summary>
    public class Params
    {
        /// <summary>
        ///     Gets or sets the type of the parameter object. Default is "object".
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "object";

        /// <summary>
        ///     Gets or sets the dictionary of parameters, where the key is the parameter name and the value is the parameter
        ///     details.
        /// </summary>
        [JsonPropertyName("parameters")]
        public Dictionary<string, Param> Parameters { get; set; } = null;

        /// <summary>
        ///     Gets or sets the list of required parameter names.
        /// </summary>
        [JsonPropertyName("required")]
        public string[] Required { get; set; } = null;
    }
}