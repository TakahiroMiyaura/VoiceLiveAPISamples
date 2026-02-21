// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a JSON Schema property definition.
    /// </summary>
    public class Param
    {
        /// <summary>
        ///     Gets or sets the type of the property (e.g., "string", "number", "integer", "boolean", "array", "object").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the description of the property.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = null;

        /// <summary>
        ///     Gets or sets the schema for array items. Used when Type is "array".
        /// </summary>
        [JsonPropertyName("items")]
        public Param Items { get; set; } = null;

        /// <summary>
        ///     Gets or sets the allowed values for enum types.
        /// </summary>
        [JsonPropertyName("enum")]
        public string[] Enum { get; set; } = null;

        /// <summary>
        ///     Gets or sets the default value for this property.
        /// </summary>
        [JsonPropertyName("default")]
        public object Default { get; set; } = null;
    }
}