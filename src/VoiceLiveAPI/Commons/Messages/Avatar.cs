// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents an avatar with properties for character, style, customization, ICE servers, and video configuration.  
/// </summary>  
public class Avatar
{
    /// <summary>  
    /// Gets or sets the character of the avatar.  
    /// </summary>  
    public string? character { get; set; }

    /// <summary>  
    /// Gets or sets the style of the avatar.  
    /// </summary>  
    public string? style { get; set; }

    /// <summary>  
    /// Gets or sets a value indicating whether the avatar is customized.  
    /// </summary>  
    public bool? customized { get; set; }

    /// <summary>  
    /// Gets or sets the ICE servers associated with the avatar.  
    /// </summary>  
    public IceServers[]? ice_servers { get; set; }

    /// <summary>  
    /// Gets or sets the video configuration for the avatar.  
    /// </summary>  
    public Video? video { get; set; }
}
