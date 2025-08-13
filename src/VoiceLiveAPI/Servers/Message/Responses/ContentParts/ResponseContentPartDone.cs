// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.ContentParts;

/// <summary>  
/// Represents a response indicating that a content part has been completed.  
/// </summary>  
public class ResponseContentPartDone : MessageBase
{
    /// <summary>  
    /// The type identifier for this response.  
    /// </summary>  
    public const string Type = "response.content_part.done";

    /// <summary>  
    /// Gets or sets the response ID.  
    /// </summary>  
    public string response_id { get; set; }

    /// <summary>  
    /// Gets or sets the item ID.  
    /// </summary>  
    public string item_id { get; set; }

    /// <summary>  
    /// Gets or sets the output index.  
    /// </summary>  
    public int output_index { get; set; }

    /// <summary>  
    /// Gets or sets the content index.  
    /// </summary>  
    public int content_index { get; set; }

    /// <summary>  
    /// Gets or sets the part information.  
    /// </summary>  
    public Part part { get; set; }

    /// <summary>  
    /// Initializes a new instance of the <see cref="ResponseContentPartDone"/> class.  
    /// </summary>  
    public ResponseContentPartDone()
    {
        type = Type;
        response_id = string.Empty;
        item_id = string.Empty;
        output_index = 0;
        content_index = 0;
        part = new Part();
    }
}
