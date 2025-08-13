// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents a video configuration with properties for bitrate, codec, crop, resolution, and background.  
/// </summary>  
public class Video
{
    /// <summary>  
    /// Gets or sets the bitrate of the video.  
    /// </summary>  
    public int? bitrate { get; set; }

    /// <summary>  
    /// Gets or sets the codec used for the video.  
    /// </summary>  
    public string? codec { get; set; }

    /// <summary>  
    /// Gets or sets the crop settings for the video.  
    /// </summary>  
    public Crop? crop { get; set; }

    /// <summary>  
    /// Gets or sets the resolution of the video.  
    /// </summary>  
    public Resolution? resolution { get; set; }

    /// <summary>  
    /// Gets or sets the background settings for the video.  
    /// </summary>  
    public Background? background { get; set; }
}
