// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents the details of output tokens, including text and audio tokens.  
/// </summary>  
public class OutputTokenDetails
{
    /// <summary>  
    /// Gets or sets the number of text tokens.  
    /// </summary>  
    public int text_tokens { get; set; }

    /// <summary>  
    /// Gets or sets the number of audio tokens.  
    /// </summary>  
    public int audio_tokens { get; set; }
}
