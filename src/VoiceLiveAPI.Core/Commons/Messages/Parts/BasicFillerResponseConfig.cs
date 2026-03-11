// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a static filler response configuration.
    /// </summary>
    /// <remarks>
    ///     Static fillers randomly select and speak text from a predefined list.
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    public class BasicFillerResponseConfig : FillerResponseConfig
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type discriminator value for static filler responses.
        /// </summary>
        public const string TypeDiscriminator = "static_filler";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the type of the filler response. Always "static_filler" for static fillers.
        /// </summary>
        [JsonPropertyName("type")]
        public override string Type { get; set; } = TypeDiscriminator;

        /// <summary>
        ///     Gets or sets the list of texts to randomly select from for the filler response.
        /// </summary>
        [JsonPropertyName("texts")]
        public string[] Texts { get; set; }

        #endregion
    }
}
