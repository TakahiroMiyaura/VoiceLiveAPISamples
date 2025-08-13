// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents the details of a status, including its type and reason.  
/// </summary>  
public class StatusDetails
{
    /// <summary>  
    /// Gets or sets the type of the status.  
    /// </summary>  
    public string? type { get; set; }

    /// <summary>  
    /// Gets or sets the reason for the status.  
    /// </summary>  
    public string? reason { get; set; }
}
