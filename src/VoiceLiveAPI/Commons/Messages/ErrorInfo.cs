// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>
///     Represents error information.
/// </summary>
public class ErrorInfo
{
    /// <summary>
    ///     Gets or sets the error code.
    /// </summary>
    public string code { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the error message.
    /// </summary>
    public string message { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the parameter that caused the error.
    /// </summary>
    public string? param { get; set; }

    /// <summary>
    ///     Gets or sets the event ID associated with the error.
    /// </summary>
    public string? event_id { get; set; }
}