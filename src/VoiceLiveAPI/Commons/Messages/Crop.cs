// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>  
    /// Represents a crop area defined by top-left and bottom-right coordinates.  
    /// </summary>  
    public class Crop
    {
        /// <summary>  
        /// Gets or sets the top-left coordinates of the crop area.  
        /// </summary>  
        public int[] top_left { get; set; } = null;

        /// <summary>  
        /// Gets or sets the bottom-right coordinates of the crop area.  
        /// </summary>  
        public int[] bottom_right { get; set; } = null;

    }
}