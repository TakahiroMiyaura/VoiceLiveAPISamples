// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents the content of a message, including its type and transcript.  
/// </summary>  
public class Content
{
    /// <summary>  
    /// Gets or sets the type of the content.  
    /// </summary>  
    public string? type { get; set; }

    /// <summary>  
    /// Gets or sets the transcript of the content.  
    /// </summary>  
    public string? transcript { get; set; }
}
