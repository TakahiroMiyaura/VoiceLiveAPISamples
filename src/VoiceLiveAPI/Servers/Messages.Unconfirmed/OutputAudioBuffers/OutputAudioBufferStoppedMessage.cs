// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.OutputAudioBuffers;

/// <summary>
///     Represents an output audio buffer stopped message.
/// </summary>
public class OutputAudioBufferStoppedMessage : VoiceLiveMessage
{
    /// <summary>
    ///     Gets or sets the audio end in milliseconds.
    /// </summary>
    public int audio_end_ms { get; set; }

    /// <summary>
    ///     Gets or sets the Item ID.
    /// </summary>
    public string item_id { get; set; } = string.Empty;
}