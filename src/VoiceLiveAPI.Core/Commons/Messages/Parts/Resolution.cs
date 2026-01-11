// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the resolution of a display or image.
    /// </summary>
    public class Resolution
    {
        /// <summary>
        ///     Gets or sets the width of the resolution.
        /// </summary>
        [JsonPropertyName("width")]
        public int Width { get; set; }

        /// <summary>
        ///     Gets or sets the height of the resolution.
        /// </summary>
        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}