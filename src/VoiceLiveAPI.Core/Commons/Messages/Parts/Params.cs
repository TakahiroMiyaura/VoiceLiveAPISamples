// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a JSON Schema for function parameters.
    /// </summary>
    public class Params
    {
        /// <summary>
        ///     Gets or sets the type of the parameter object. Default is "object".
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "object";

        /// <summary>
        ///     Gets or sets the dictionary of properties, where the key is the property name and the value is the property
        ///     schema.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, Param> Properties { get; set; } = null;

        /// <summary>
        ///     Gets or sets the list of required property names.
        /// </summary>
        [JsonPropertyName("required")]
        public string[] Required { get; set; } = null;

        /// <summary>
        ///     Gets or sets whether additional properties are allowed. Default is null (not specified).
        /// </summary>
        [JsonPropertyName("additionalProperties")]
        public bool? AdditionalProperties { get; set; } = null;
    }
}