// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a crop area defined by top-left and bottom-right coordinates.
    /// </summary>
    public class Crop
    {
        /// <summary>
        ///     Gets or sets the top-left coordinates of the crop area.
        /// </summary>
        [JsonPropertyName("top_left")]
        public int[] TopLeft { get; set; } = null;

        /// <summary>
        ///     Gets or sets the bottom-right coordinates of the crop area.
        /// </summary>
        [JsonPropertyName("bottom_right")]
        public int[] BottomRight { get; set; } = null;
    }
}