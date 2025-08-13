// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Sessions;

/// <summary>  
/// Represents an updated server session message in the VoiceLiveAPI.  
/// </summary>  
public class ServerSessionUpdated : MessageBase
{
    /// <summary>  
    /// The type identifier for the updated session message.  
    /// </summary>  
    public const string Type = "session.updated";

    /// <summary>  
    /// Gets or sets the server session associated with the update.  
    /// </summary>  
    public ServerSession session { get; set; }

    /// <summary>  
    /// Initializes a new instance of the <see cref="ServerSessionUpdated"/> class.  
    /// </summary>  
    public ServerSessionUpdated()
    {
        type = Type;
        session = new ServerSession();
    }
}
