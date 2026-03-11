// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents an LLM-based filler response configuration.
    /// </summary>
    /// <remarks>
    ///     LLM fillers dynamically generate context-appropriate responses using a language model.
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    public class LlmFillerResponseConfig : FillerResponseConfig
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type discriminator value for LLM filler responses.
        /// </summary>
        public const string TypeDiscriminator = "llm_filler";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the type of the filler response. Always "llm_filler" for LLM fillers.
        /// </summary>
        [JsonPropertyName("type")]
        public override string Type { get; set; } = TypeDiscriminator;

        /// <summary>
        ///     Gets or sets the model to use for filler response generation.
        ///     Defaults to "gpt-4.1-mini".
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        ///     Gets or sets the custom instructions for filler response generation.
        /// </summary>
        [JsonPropertyName("instructions")]
        public string Instructions { get; set; }

        /// <summary>
        ///     Gets or sets the maximum number of completion tokens for the filler response.
        ///     Defaults to 50.
        /// </summary>
        [JsonPropertyName("max_completion_tokens")]
        public int? MaxCompletionTokens { get; set; }

        #endregion
    }
}
