// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Conversations.Items;

/// <summary>  
/// Represents a message indicating the completion of audio transcription for a conversation item.  
/// </summary>  
public class ConversationItemInputAudioTranscriptionCompleted : MessageBase
{
    /// <summary>  
    /// The type identifier for this message.  
    /// </summary>  
    public const string Type = "conversation.item.input_audio_transcription.completed";

    /// <summary>  
    /// Gets or sets the unique identifier of the conversation item.  
    /// </summary>  
    public string item_id { get; set; }

    /// <summary>  
    /// Gets or sets the index of the content within the conversation item.  
    /// </summary>  
    public int content_index { get; set; }

    /// <summary>  
    /// Gets or sets the transcript of the audio content.  
    /// </summary>  
    public string transcript { get; set; }

    /// <summary>  
    /// Initializes a new instance of the <see cref="ConversationItemInputAudioTranscriptionCompleted"/> class.  
    /// </summary>  
    public ConversationItemInputAudioTranscriptionCompleted()
    {
        event_id = string.Empty;
        type = Type;
        item_id = string.Empty;
        content_index = 0;
        transcript = string.Empty;
    }
}
