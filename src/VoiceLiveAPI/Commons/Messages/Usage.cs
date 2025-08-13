// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents the usage details including token counts and their details.  
/// </summary>  
public class Usage
{
    /// <summary>  
    /// Gets or sets the total number of tokens used.  
    /// </summary>  
    public int? total_tokens { get; set; }

    /// <summary>  
    /// Gets or sets the number of input tokens used.  
    /// </summary>  
    public int? input_tokens { get; set; }

    /// <summary>  
    /// Gets or sets the number of output tokens used.  
    /// </summary>  
    public int? output_tokens { get; set; }

    /// <summary>  
    /// Gets or sets the details of input tokens.  
    /// </summary>  
    public InputTokenDetails? input_token_details { get; set; }

    /// <summary>  
    /// Gets or sets the details of output tokens.  
    /// </summary>  
    public OutputTokenDetails? output_token_details { get; set; }
}
