// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message;

/// <summary>  
/// Represents the base class for messages in the VoiceLiveAPI.  
/// </summary>  
public class MessageBase
{
    /// <summary>  
    /// Gets or sets the unique identifier for the event.  
    /// </summary>  
    public string? event_id { get; set; }

    /// <summary>  
    /// Gets or sets the type of the message.  
    /// </summary>  
    public string? type { get; set; }
}
