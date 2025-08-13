// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents a part of a message with a type and transcript.  
/// </summary>  
public class Part
{
    /// <summary>  
    /// Gets or sets the type of the part.  
    /// </summary>  
    public string? type { get; set; }

    /// <summary>  
    /// Gets or sets the transcript of the part.  
    /// </summary>  
    public string? transcript { get; set; }
}
