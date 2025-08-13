// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.Responses.FunctionCallArguments;

/// <summary>
///     Represents a response function call arguments done message.
/// </summary>
public class ResponseFunctionCallArgumentsDoneMessage : VoiceLiveMessage
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
    ///     Gets or sets the call ID.
    /// </summary>
    public string call_id { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the complete arguments.
    /// </summary>
    public string arguments { get; set; } = string.Empty;
}