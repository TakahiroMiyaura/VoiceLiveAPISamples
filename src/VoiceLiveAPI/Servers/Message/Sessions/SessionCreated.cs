// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Sessions;

/// <summary>  
/// Represents a message indicating that a session has been created.  
/// </summary>  
public class SessionCreated : MessageBase
{
    /// <summary>  
    /// The type of the message.  
    /// </summary>  
    public const string Type = "session.created";

    /// <summary>  
    /// The session information associated with the created session.  
    /// </summary>  
    public ServerSession session { get; set; }

    /// <summary>  
    /// Initializes a new instance of the <see cref="SessionCreated"/> class.  
    /// </summary>  
    public SessionCreated()
    {
        event_id = Guid.NewGuid().ToString();
        type = Type;
        session = new ServerSession();
    }
}
