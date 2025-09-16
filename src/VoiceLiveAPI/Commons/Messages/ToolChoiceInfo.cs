// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>
    /// Represents information about a tool choice.
    /// </summary>
    public class ToolChoiceInfo
    {
        /// <summary>
        /// Gets or sets the literal value associated with the tool choice.
        /// </summary>
        public string literal { get; set; } = null;

        /// <summary>
        /// Gets or sets the type of the tool choice. Allowed values: "function".
        /// </summary>
        public string type { get; set; } = null;

        /// <summary>
        /// Gets or sets the function information for the tool choice.
        /// </summary>
        public FunctionInfo function { get; set; } = null;
    }
}