// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.InputAudioBuffers;

/// <summary>  
/// Represents a message indicating that speech has stopped in the input audio buffer.  
/// </summary>  
public class InputAudioBufferSpeechStopped : MessageBase
{
    /// <summary>  
    /// The type identifier for this message.  
    /// </summary>  
    public const string Type = "input_audio_buffer.speech_stopped";

    /// <summary>  
    /// Gets or sets the timestamp (in milliseconds) indicating the end of the audio.  
    /// </summary>  
    public int audio_end_ms { get; set; }

    /// <summary>  
    /// Gets or sets the identifier for the audio item.  
    /// </summary>  
    public string item_id { get; set; }

    /// <summary>  
    /// Initializes a new instance of the <see cref="InputAudioBufferSpeechStopped"/> class.  
    /// </summary>  
    public InputAudioBufferSpeechStopped()
    {
        event_id = string.Empty;
        type = Type;
        audio_end_ms = 0;
        item_id = string.Empty;
    }
}
