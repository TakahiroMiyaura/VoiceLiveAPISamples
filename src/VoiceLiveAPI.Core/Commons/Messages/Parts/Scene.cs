// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents scene parameters for avatar rendering (zoom, position, rotation and amplitude).
    /// </summary>
    /// <remarks>
    ///     Mirrors the service's avatar <c>scene</c> object. Available in API version 2025-10-01 and later.
    /// </remarks>
    public class Scene
    {
        /// <summary>
        ///     Gets or sets the zoom factor of the scene.
        /// </summary>
        [JsonPropertyName("zoom")]
        public float? Zoom { get; set; }

        /// <summary>
        ///     Gets or sets the horizontal position of the avatar within the scene.
        /// </summary>
        [JsonPropertyName("position_x")]
        public float? PositionX { get; set; }

        /// <summary>
        ///     Gets or sets the vertical position of the avatar within the scene.
        /// </summary>
        [JsonPropertyName("position_y")]
        public float? PositionY { get; set; }

        /// <summary>
        ///     Gets or sets the rotation around the X axis.
        /// </summary>
        [JsonPropertyName("rotation_x")]
        public float? RotationX { get; set; }

        /// <summary>
        ///     Gets or sets the rotation around the Y axis.
        /// </summary>
        [JsonPropertyName("rotation_y")]
        public float? RotationY { get; set; }

        /// <summary>
        ///     Gets or sets the rotation around the Z axis.
        /// </summary>
        [JsonPropertyName("rotation_z")]
        public float? RotationZ { get; set; }

        /// <summary>
        ///     Gets or sets the motion amplitude of the avatar.
        /// </summary>
        [JsonPropertyName("amplitude")]
        public float? Amplitude { get; set; }
    }
}
