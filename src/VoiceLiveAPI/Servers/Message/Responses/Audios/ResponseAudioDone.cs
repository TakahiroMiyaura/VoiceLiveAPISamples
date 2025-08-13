// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.Audios;

/// <summary>  
/// Represents a response indicating that audio processing is completed.  
/// </summary>  
public class ResponseAudioDone : MessageBase
{
    /// <summary>  
    /// The type identifier for this response.  
    /// </summary>  
    public const string Type = "response.audio.done";

    /// <summary>  
    /// Gets or sets the unique identifier for the response.  
    /// </summary>  
    public string response_id { get; set; }

    /// <summary>  
    /// Gets or sets the unique identifier for the item.  
    /// </summary>  
    public string item_id { get; set; }

    /// <summary>  
    /// Gets or sets the index of the output.  
    /// </summary>  
    public int output_index { get; set; }

    /// <summary>  
    /// Gets or sets the index of the content.  
    /// </summary>  
    public int content_index { get; set; }

    /// <summary>  
    /// Initializes a new instance of the <see cref="ResponseAudioDone"/> class.  
    /// </summary>  
    public ResponseAudioDone()
    {
        event_id = string.Empty;
        type = Type;
        response_id = string.Empty;
        item_id = string.Empty;
        output_index = 0;
        content_index = 0;
    }
}
