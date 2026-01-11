// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages.Unverified
{
    /// <summary>
    ///     Represents a response create message.
    /// </summary>
    public class ResponseCreateMessage : ClientCommand
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "response.create";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the response configuration.
        /// </summary>
        [JsonPropertyName("response")]
        public ResponseOptionsMessage Response { get; set; } = null;

        #endregion
    }

    /// <summary>
    ///     Represents the options for a response message.
    /// </summary>
    public class ResponseOptionsMessage
    {
        /// <summary>
        ///     Gets or sets the modalities of the response.
        ///     Possible values: text, audio.
        /// </summary>
        [JsonPropertyName("modalities")]
        public string[] Modalities { get; set; } = null;

        /// <summary>
        ///     Gets or sets the instructions for the response.
        /// </summary>
        [JsonPropertyName("instructions")]
        public string Instructions { get; set; } = null;

        /// <summary>
        ///     Gets or sets the voice type for the response.
        ///     Possible values: alloy, ash, ballad, coral, echo, sage, shimmer, verse.
        /// </summary>
        [JsonPropertyName("voice")]
        public string Voice { get; set; } = null;

        /// <summary>
        ///     Gets or sets the output audio format.
        ///     Possible values: pcm16, g711_ulaw, g711_alaw.
        /// </summary>
        [JsonPropertyName("output_audio_format")]
        public string OutputAudioFormat { get; set; } = null;

        /// <summary>
        ///     Gets or sets the tools used in the response.
        /// </summary>
        [JsonPropertyName("tools")]
        public ToolInfo[] Tools { get; set; } = null;

        /// <summary>
        ///     Gets or sets the tool choice information.
        /// </summary>
        [JsonPropertyName("tool_choice")]
        public ToolChoiceInfo ToolChoice { get; set; } = null;

        /// <summary>
        ///     Gets or sets the temperature for the response generation.
        ///     Default value: 0.8.
        /// </summary>
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.8;

        /// <summary>
        ///     Gets or sets the maximum output tokens for the response.
        ///     Can be an integer or "inf".
        ///     Default value: "inf".
        /// </summary>
        [JsonPropertyName("max__output_tokens")]
        public object MaxOutputTokens { get; set; } = "inf";

        /// <summary>
        ///     Gets or sets the conversation mode.
        ///     Default value: "auto".
        /// </summary>
        [JsonPropertyName("conversation")]
        public string Conversation { get; set; } = "auto";

        /// <summary>
        ///     Gets or sets the metadata associated with the response.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = null;

        /// <summary>
        ///     Gets or sets the input conversation items.
        /// </summary>
        [JsonPropertyName("input")]
        public ConversationItemBaseInfo[] Input { get; set; } = null;
    }
}