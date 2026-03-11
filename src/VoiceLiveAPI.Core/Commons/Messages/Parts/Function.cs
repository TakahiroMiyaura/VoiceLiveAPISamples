// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a function tool definition for the Realtime API.
    /// </summary>
    public class Function : RealtimeTool
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type discriminator value for function tools.
        /// </summary>
        public const string TypeDiscriminator = "function";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the type of the tool. Always "function" for function tools.
        /// </summary>
        [JsonPropertyName("type")]
        public override string Type { get; set; } = TypeDiscriminator;

        /// <summary>
        ///     Gets or sets the name of the function to use.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null;

        /// <summary>
        ///     Gets or sets the description of the function.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = null;

        /// <summary>
        ///     Gets or sets the parameters of the function.
        /// </summary>
        [JsonPropertyName("parameters")]
        public Params Parameters { get; set; } = null;

        #endregion
    }
}