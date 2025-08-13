// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents an item with various properties such as ID, type, status, role, and content.  
/// </summary>  
public class Item
{
    /// <summary>  
    /// Gets or sets the unique identifier of the item.  
    /// </summary>  
    public string? id { get; set; }

    /// <summary>  
    /// Gets or sets the type of the item.  
    /// </summary>  
    public string? type { get; set; }

    /// <summary>  
    /// Gets or sets the status of the item.  
    /// </summary>  
    public string? status { get; set; }

    /// <summary>  
    /// Gets or sets the role associated with the item.  
    /// </summary>  
    public string? role { get; set; }

    /// <summary>  
    /// Gets or sets the content associated with the item.  
    /// </summary>  
    public Content[]? content { get; set; }
}
