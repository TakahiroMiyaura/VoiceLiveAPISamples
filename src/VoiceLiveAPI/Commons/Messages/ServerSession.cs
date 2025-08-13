// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents a server session that extends the client session with additional properties.  
/// </summary>  
public class ServerSession : ClientSession
{
    /// <summary>  
    /// Gets or sets the instructions for the server session.  
    /// </summary>  
    public string? instructions { get; set; }

    /// <summary>  
    /// Gets or sets the tool choice for the server session.  
    /// </summary>  
    public new string? tool_choice { get; set; }

    /// <summary>  
    /// Gets or sets the temperature value for the server session.  
    /// </summary>  
    public float? temperture { get; set; }

    /// <summary>  
    /// Gets or sets the agent information for the server session.  
    /// </summary>  
    public Agent? agent { get; set; }
}
