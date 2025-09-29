// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified
{
    /// <summary>
    ///     Represents information about a function, including its name, description, and parameters.
    /// </summary>
    public class FunctionInfo
    {
        /// <summary>
        ///     The name of the function to use.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null;

        /// <summary>
        ///     A description of the function.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = null;

        /// <summary>
        ///     The parameters required by the function.
        /// </summary>
        [JsonPropertyName("parameters")]
        public object[] Parameters { get; set; } = null;
    }
}