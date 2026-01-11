// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified
{
    /// <summary>
    ///     Represents information about a tool choice.
    /// </summary>
    public class ToolChoiceInfo
    {
        /// <summary>
        ///     Gets or sets the literal value associated with the tool choice.
        /// </summary>
        [JsonPropertyName("literal")]
        public string Literal { get; set; } = null;

        /// <summary>
        ///     Gets or sets the type of the tool choice. Allowed values: "function".
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the function information for the tool choice.
        /// </summary>
        [JsonPropertyName("function")]
        public FunctionInfo Function { get; set; } = null;
    }
}