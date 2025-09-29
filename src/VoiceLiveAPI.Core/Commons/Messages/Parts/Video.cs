// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a video configuration with properties for bitrate, codec, crop, resolution, and background.
    /// </summary>
    public class Video
    {
        /// <summary>
        ///     Gets or sets the bitrate of the video.
        /// </summary>
        [JsonPropertyName("bitrate")]
        public int? BitRate { get; set; }

        /// <summary>
        ///     Gets or sets the codec used for the video.
        /// </summary>
        [JsonPropertyName("codec")]
        public string Codec { get; set; } = null;

        /// <summary>
        ///     Gets or sets the crop settings for the video.
        /// </summary>
        [JsonPropertyName("crop")]
        public Crop Crop { get; set; } = null;

        /// <summary>
        ///     Gets or sets the resolution of the video.
        /// </summary>
        [JsonPropertyName("resolution")]
        public Resolution Resolution { get; set; } = null;

        /// <summary>
        ///     Gets or sets the background settings for the video.
        /// </summary>
        [JsonPropertyName("background")]
        public Background Background { get; set; } = null;
    }
}