// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents an animation with a collection of output strings.
    /// </summary>
    public class Animation
    {
        /// <summary>
        ///     Gets or sets the output strings for the animation.
        /// </summary>
        [JsonPropertyName("outputs")]
        public string[] Outputs { get; set; } = null;
    }
}