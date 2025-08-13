// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>
/// Represents information about a function, including its name, description, and parameters.
/// </summary>
public class FunctionInfo
{
    /// <summary>
    /// The name of the function to use.
    /// </summary>
    public string? name { get; set; }

    /// <summary>
    /// A description of the function.
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// The parameters required by the function.
    /// </summary>
    public object[]? parameters { get; set; }
}
