// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.OutputAudioBuffers;

/// <summary>  
///     Represents an output audio buffer started message.  
/// </summary>  
public class OutputAudioBufferStartedMessage : VoiceLiveMessage
{
    /// <summary>  
    ///     Gets or sets the event ID associated with the message.  
    /// </summary>  
    public string? event_id;

    /// <summary>  
    ///     Gets or sets the response ID associated with the message.  
    /// </summary>  
    public string? response_id;
}
