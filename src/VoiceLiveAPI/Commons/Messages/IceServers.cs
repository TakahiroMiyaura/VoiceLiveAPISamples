// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents the configuration for ICE (Interactive Connectivity Establishment) servers.  
/// </summary>  
public class IceServers
{
    /// <summary>  
    /// Gets or sets the URLs of the ICE servers.  
    /// </summary>  
    public string[]? urls { get; set; }

    /// <summary>  
    /// Gets or sets the username for the ICE server authentication.  
    /// </summary>  
    public string? username { get; set; }

    /// <summary>  
    /// Gets or sets the credential for the ICE server authentication.  
    /// </summary>  
    public string? credential { get; set; }
}
