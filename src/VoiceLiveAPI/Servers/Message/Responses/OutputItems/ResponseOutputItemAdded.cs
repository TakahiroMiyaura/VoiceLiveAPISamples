// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.OutputItems;

/// <summary>  
/// Represents a response indicating that an output item has been added.  
/// </summary>  
public class ResponseOutputItemAdded : MessageBase
{
    /// <summary>  
    /// The type identifier for this response.  
    /// </summary>  
    public const string Type = "response.output_item.added";

    /// <summary>  
    /// Gets or sets the response ID.  
    /// </summary>  
    public string response_id { get; set; }

    /// <summary>  
    /// Gets or sets the index of the output item.  
    /// </summary>  
    public int output_index { get; set; }

    /// <summary>  
    /// Gets or sets the item associated with the response.  
    /// </summary>  
    public Item item { get; set; }

    /// <summary>  
    /// Initializes a new instance of the <see cref="ResponseOutputItemAdded"/> class.  
    /// </summary>  
    public ResponseOutputItemAdded()
    {
        event_id = string.Empty;
        type = Type;
        response_id = string.Empty;
        output_index = 0;
        item = new Item();
    }
}
