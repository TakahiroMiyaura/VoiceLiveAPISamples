// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents the background settings, including color and image URL.  
/// </summary>  
public class Background
{
    /// <summary>  
    /// Gets or sets the background color.  
    /// </summary>  
    public string? color { get; set; }

    /// <summary>  
    /// Gets or sets the URL of the background image.  
    /// </summary>  
    public string? image_url { get; set; }
}
