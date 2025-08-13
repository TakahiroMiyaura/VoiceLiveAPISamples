// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0


namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>
/// Represents the settings for echo cancellation applied to the audio input.
/// </summary>
public class AudioInputEchoCancellationSettings
{
    /// <summary>  
    /// Gets or sets the type of noise reduction applied to the audio input.  
    /// </summary>  
    public string? type { get; set; }
}
