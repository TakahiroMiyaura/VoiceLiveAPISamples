// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>  
/// Represents a client session configuration for the VoiceLiveAPI.  
/// </summary>  
public class ClientSession
{
    /// <summary>  
    /// Gets or sets the modalities supported by the client session.  
    /// </summary>  
    public string[]? modalities { get; set; }

    /// <summary>  
    /// Gets or sets the input audio format. Supported formats: pcm16, g711_ulaw, g711_alaw.  
    /// </summary>  
    public string? input_audio_format { get; set; }

    /// <summary>  
    /// Gets or sets the output audio format. Supported formats: pcm16, g711_ulaw, g711_alaw.  
    /// </summary>  
    public string? output_audio_format { get; set; }

    /// <summary>  
    /// Gets or sets the settings for input audio noise reduction.  
    /// </summary>  
    public AudioInputAudioNoiseReductionSettings? input_audio_noise_reduction { get; set; }

    /// <summary>  
    /// Gets or sets the settings for input audio transcription.  
    /// </summary>  
    public AudioInputTranscriptionSettings? input_audio_transcription { get; set; }
    /// <summary>  
    /// Gets or sets the settings for input audio echo cancellation.  
    /// </summary>  
    public AudioInputEchoCancellationSettings? input_audio_echo_cancellation { get; set; }
    /// <summary>  
    /// Gets or sets the configuration for turn detection.  
    /// </summary>  
    public TurnDetection? turn_detection { get; set; }

    /// <summary>  
    /// Gets or sets the tools available for the client session.  
    /// </summary>  
    public Function[]? tools { get; set; }

    /// <summary>  
    /// Gets or sets the tool choice for the client session.  
    /// </summary>  
    public string? tool_choice { get; set; }

    /// <summary>  
    /// Gets or sets the maximum number of response output tokens. Can be an integer or "inf".  
    /// </summary>  
    public string? max_response_output_tokens { get; set; }

    /// <summary>  
    /// Gets or sets the model used for the client session.  
    /// </summary>  
    public string? model { get; set; }

    /// <summary>  
    /// Gets or sets the voice configuration for the client session.  
    /// </summary>  
    public Voice? voice { get; set; }

    /// <summary>  
    /// Gets or sets the animation settings for the client session.  
    /// </summary>  
    public Animation? animation { get; set; }

    /// <summary>  
    /// Gets or sets the avatar configuration for the client session.  
    /// </summary>  
    public Avatar? avatar { get; set; }

    /// <summary>  
    /// Gets or sets the types of output audio timestamps.  
    /// </summary>  
    public string[]? output_audio_timestamp_types { get; set; }

    /// <summary>  
    /// Gets or sets the input audio sampling rate.  
    /// </summary>  
    public int? input_audio_sampling_rate { get; set; }

}