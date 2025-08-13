// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Responses.Texts;

/// <summary>
///     Represents a response text delta message.
/// </summary>
public class ResponseTextDeltaMessage : VoiceLiveMessage
{
    /// <summary>
    ///     Gets or sets the response ID.
    /// </summary>
    public string response_id { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the Item ID.
    /// </summary>
    public string item_id { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the output index.
    /// </summary>
    public int output_index { get; set; }

    /// <summary>
    ///     Gets or sets the content index.
    /// </summary>
    public int content_index { get; set; }

    /// <summary>
    ///     Gets or sets the text delta.
    /// </summary>
    public string delta { get; set; } = string.Empty;
}