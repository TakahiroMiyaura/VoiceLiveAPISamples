// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents the background settings, including color and image URL.
    /// </summary>
    public class Background
    {
        /// <summary>
        ///     Gets or sets the background color.
        /// </summary>
        [JsonPropertyName("color")]
        public string Color { get; set; } = null;

        /// <summary>
        ///     Gets or sets the URL of the background image.
        /// </summary>
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = null;
    }
}