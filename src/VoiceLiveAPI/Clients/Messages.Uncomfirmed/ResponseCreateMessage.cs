// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Uncomfirmed;

/// <summary>
///     Represents a response create message.
/// </summary>
public class ResponseCreateMessage : VoiceLiveMessage
{
    /// <summary>
    ///     Gets or sets the response configuration.
    /// </summary>
    public ResponseOptionsMessage? response { get; set; }
}

/// <summary>  
/// Represents the options for a response message.  
/// </summary>  
public class ResponseOptionsMessage
{
    /// <summary>  
    /// Gets or sets the modalities of the response.  
    /// Possible values: text, audio.  
    /// </summary>  
    public string[]? modalities { get; set; }

    /// <summary>  
    /// Gets or sets the instructions for the response.  
    /// </summary>  
    public string? instructions { get; set; }

    /// <summary>  
    /// Gets or sets the voice type for the response.  
    /// Possible values: alloy, ash, ballad, coral, echo, sage, shimmer, verse.  
    /// </summary>  
    public string? voice { get; set; }

    /// <summary>  
    /// Gets or sets the output audio format.  
    /// Possible values: pcm16, g711_ulaw, g711_alaw.  
    /// </summary>  
    public string? output_audio_format { get; set; }

    /// <summary>  
    /// Gets or sets the tools used in the response.  
    /// </summary>  
    public ToolInfo[]? tools { get; set; }

    /// <summary>  
    /// Gets or sets the tool choice information.  
    /// </summary>  
    public ToolChoiceInfo? tool_choice { get; set; }

    /// <summary>  
    /// Gets or sets the temperature for the response generation.  
    /// Default value: 0.8.  
    /// </summary>  
    public double temperature { get; set; } = 0.8;

    /// <summary>  
    /// Gets or sets the maximum output tokens for the response.  
    /// Can be an integer or "inf".  
    /// Default value: "inf".  
    /// </summary>  
    public object max__output_tokens { get; set; } = "inf";

    /// <summary>  
    /// Gets or sets the conversation mode.  
    /// Default value: "auto".  
    /// </summary>  
    public string conversation { get; set; } = "auto";

    /// <summary>  
    /// Gets or sets the metadata associated with the response.  
    /// </summary>  
    public Dictionary<string, string>? metadata { get; set; }

    /// <summary>  
    /// Gets or sets the input conversation items.  
    /// </summary>  
    public ConversationItemBaseInfo[]? input { get; set; }
}
