// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.ContentParts;

/// <summary>  
/// Represents a response indicating that a content part has been added.  
/// </summary>  
public class ResponseContentPartAdded : MessageBase
{
    /// <summary>  
    /// The type identifier for this message.  
    /// </summary>  
    public const string Type = "response.content_part.added";

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
    /// Gets or sets the part of the content.  
    /// </summary>  
    public Part part { get; set; }

    /// <summary>  
    /// Initializes a new instance of the <see cref="ResponseContentPartAdded"/> class.  
    /// </summary>  
    public ResponseContentPartAdded()
    {
        type = Type;
        response_id = string.Empty;
        item_id = string.Empty;
        output_index = 0;
        content_index = 0;
        part = new Part();
    }
}
