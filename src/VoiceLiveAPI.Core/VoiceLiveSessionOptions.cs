// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core
{
    /// <summary>
    ///     Represents the options for configuring a <see cref="VoiceLiveSession" />.
    /// </summary>
    /// <remarks>
    ///     This class provides session-level configuration options including model selection,
    ///     voice settings, turn detection, and audio format settings.
    ///     Implements <see cref="IClientSessionUpdate" /> for backward compatibility.
    /// </remarks>
    public class VoiceLiveSessionOptions : IClientSessionUpdate
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The message type for session update.
        /// </summary>
        public const string TypeName = "session.update";

        /// <summary>
        ///     The default AI model to use.
        /// </summary>
        public const string DefaultModel = "gpt-4o";

        /// <summary>
        ///     The default input audio format.
        /// </summary>
        public const string DefaultInputAudioFormat = "pcm16";

        /// <summary>
        ///     The default output audio format.
        /// </summary>
        public const string DefaultOutputAudioFormat = "pcm16";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the message type identifier.
        /// </summary>
        /// <remarks>
        ///     This property is excluded from JSON serialization because when VoiceLiveSessionOptions
        ///     is used inside a session.update message, the type field should only appear at the
        ///     top level, not inside the session object.
        /// </remarks>
        [JsonIgnore]
        public string Type => TypeName;

        /// <summary>
        ///     Gets or sets the AI model to use for the session.
        /// </summary>
        /// <value>
        ///     The model identifier (e.g., "gpt-4o", "gpt-4o-mini"). Defaults to <see cref="DefaultModel" />.
        /// </value>
        [JsonPropertyName("model")]
        public string Model { get; set; } = DefaultModel;

        /// <summary>
        ///     Gets or sets the modalities supported by the session.
        /// </summary>
        /// <value>
        ///     An array of modality strings (e.g., ["audio", "text"]). Defaults to ["audio", "text"].
        /// </value>
        [JsonPropertyName("modalities")]
        public string[] Modalities { get; set; } = { "audio", "text" };

        /// <summary>
        ///     Gets or sets the system instructions for the AI model.
        /// </summary>
        /// <value>
        ///     The instructions string that defines the AI assistant's behavior.
        /// </value>
        [JsonPropertyName("instructions")]
        public string Instructions { get; set; }

        /// <summary>
        ///     Gets or sets the voice configuration for audio output.
        /// </summary>
        /// <value>
        ///     A <see cref="Parts.Voice" /> object containing voice settings.
        /// </value>
        [JsonPropertyName("voice")]
        public Voice Voice { get; set; }

        /// <summary>
        ///     Gets or sets the turn detection configuration.
        /// </summary>
        /// <value>
        ///     A <see cref="Parts.TurnDetection" /> object containing turn detection settings.
        /// </value>
        [JsonPropertyName("turn_detection")]
        public TurnDetection TurnDetection { get; set; }

        /// <summary>
        ///     Gets or sets the input audio format.
        /// </summary>
        /// <value>
        ///     The audio format string (e.g., "pcm16", "g711_ulaw", "g711_alaw").
        ///     Defaults to <see cref="DefaultInputAudioFormat" />.
        /// </value>
        [JsonPropertyName("input_audio_format")]
        public string InputAudioFormat { get; set; } = DefaultInputAudioFormat;

        /// <summary>
        ///     Gets or sets the output audio format.
        /// </summary>
        /// <value>
        ///     The audio format string (e.g., "pcm16", "g711_ulaw", "g711_alaw").
        ///     Defaults to <see cref="DefaultOutputAudioFormat" />.
        /// </value>
        [JsonPropertyName("output_audio_format")]
        public string OutputAudioFormat { get; set; } = DefaultOutputAudioFormat;

        /// <summary>
        ///     Gets or sets the input audio transcription settings.
        /// </summary>
        /// <value>
        ///     An <see cref="AudioInputTranscriptionSettings" /> object for transcription configuration.
        /// </value>
        [JsonPropertyName("input_audio_transcription")]
        public AudioInputTranscriptionSettings InputAudioTranscription { get; set; }

        /// <summary>
        ///     Gets or sets the input audio noise reduction settings.
        /// </summary>
        /// <value>
        ///     An <see cref="AudioInputAudioNoiseReductionSettings" /> object for noise reduction configuration.
        /// </value>
        [JsonPropertyName("input_audio_noise_reduction")]
        public AudioInputAudioNoiseReductionSettings InputAudioNoiseReduction { get; set; }

        /// <summary>
        ///     Gets or sets the input audio echo cancellation settings.
        /// </summary>
        /// <value>
        ///     An <see cref="AudioInputEchoCancellationSettings" /> object for echo cancellation configuration.
        /// </value>
        [JsonPropertyName("input_audio_echo_cancellation")]
        public AudioInputEchoCancellationSettings InputAudioEchoCancellation { get; set; }

        /// <summary>
        ///     Gets or sets the input audio sampling rate.
        /// </summary>
        /// <value>
        ///     The sampling rate in Hz (e.g., 16000, 24000, 48000).
        /// </value>
        [JsonPropertyName("input_audio_sampling_rate")]
        public int? InputAudioSamplingRate { get; set; }

        /// <summary>
        ///     Gets or sets the tools (functions) available for the AI model to call.
        /// </summary>
        /// <value>
        ///     An array of <see cref="Function" /> objects defining available tools.
        /// </value>
        [JsonPropertyName("tools")]
        public Function[] Tools { get; set; }

        /// <summary>
        ///     Gets or sets the tool choice preference.
        /// </summary>
        /// <value>
        ///     The tool choice string (e.g., "auto", "none", or a specific tool name).
        /// </value>
        [JsonPropertyName("tool_choice")]
        public string ToolChoice { get; set; }

        /// <summary>
        ///     Gets or sets the temperature for response generation.
        /// </summary>
        /// <value>
        ///     A value between 0.0 and 2.0 controlling randomness. Lower values are more deterministic.
        /// </value>
        [JsonPropertyName("temperature")]
        public float? Temperature { get; set; }

        /// <summary>
        ///     Gets or sets the maximum number of output tokens for responses.
        /// </summary>
        /// <value>
        ///     The maximum tokens as a string (e.g., "4096" or "inf").
        /// </value>
        [JsonPropertyName("max_response_output_tokens")]
        public string MaxResponseOutputTokens { get; set; }

        /// <summary>
        ///     Gets or sets the animation settings for the session.
        /// </summary>
        /// <value>
        ///     An <see cref="Parts.Animation" /> object for animation configuration.
        /// </value>
        [JsonPropertyName("animation")]
        public Animation Animation { get; set; }

        /// <summary>
        ///     Gets or sets the avatar configuration for the session.
        /// </summary>
        /// <value>
        ///     An <see cref="Parts.Avatar" /> object for avatar configuration.
        /// </value>
        [JsonPropertyName("avatar")]
        public Avatar Avatar { get; set; }

        /// <summary>
        ///     Gets or sets the output audio timestamp types.
        /// </summary>
        /// <value>
        ///     An array of timestamp type strings.
        /// </value>
        [JsonPropertyName("output_audio_timestamp_types")]
        public string[] OutputAudioTimestampTypes { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates a new instance with default settings for Azure AI VoiceLive API.
        /// </summary>
        /// <returns>A <see cref="VoiceLiveSessionOptions" /> configured for voice interaction.</returns>
        /// <remarks>
        ///     <para>
        ///         This method returns settings equivalent to <c>ClientSessionUpdate.Default</c>,
        ///         optimized for Japanese Azure voice with noise reduction and animation support.
        ///     </para>
        ///     <para>
        ///         Default settings include:
        ///         <list type="bullet">
        ///             <item>Voice: ja-JP-Nanami:DragonHDLatestNeural (azure-standard)</item>
        ///             <item>Modalities: audio, text</item>
        ///             <item>Audio format: PCM16 at 24kHz</item>
        ///             <item>Noise reduction: Azure Deep Noise Suppression</item>
        ///             <item>Turn detection: server_vad with auto-response</item>
        ///             <item>Animation: viseme_id output</item>
        ///         </list>
        ///     </para>
        /// </remarks>
        public static VoiceLiveSessionOptions CreateDefault()
        {
            return new VoiceLiveSessionOptions
            {
                Model = DefaultModel,
                Modalities = new[] { "text", "audio" },
                InputAudioFormat = DefaultInputAudioFormat,
                OutputAudioFormat = DefaultOutputAudioFormat,
                InputAudioSamplingRate = 24000,
                Voice = new Voice
                {
                    Name = "ja-JP-Nanami:DragonHDLatestNeural",
                    Type = "azure-standard"
                },
                TurnDetection = new TurnDetection
                {
                    Type = "server_vad",
                    Threshold = 0.5f,
                    SilenceDurationMs = 500,
                    CreateResponse = true
                },
                InputAudioNoiseReduction = new AudioInputAudioNoiseReductionSettings
                {
                    Type = "azure_deep_noise_suppression"
                },
                OutputAudioTimestampTypes = new[] { "word" },
                Animation = new Animation
                {
                    Outputs = new[] { "viseme_id" }
                }
            };
        }

        /// <summary>
        ///     Creates a new instance with settings optimized for Azure Voice.
        /// </summary>
        /// <param name="voiceName">The Azure voice name (e.g., "ja-JP-NanamiNeural", "en-US-JennyNeural").</param>
        /// <param name="voiceType">The voice type (default: "azure-standard").</param>
        /// <returns>A <see cref="VoiceLiveSessionOptions" /> configured for Azure voice.</returns>
        public static VoiceLiveSessionOptions CreateWithAzureVoice(string voiceName,
            string voiceType = "azure-standard")
        {
            var options = CreateDefault();
            options.Voice = new Voice
            {
                Name = voiceName,
                Type = voiceType
            };
            return options;
        }

        /// <summary>
        ///     Creates a new instance with minimal settings for basic operation.
        /// </summary>
        /// <returns>A <see cref="VoiceLiveSessionOptions" /> with minimal configuration.</returns>
        /// <remarks>
        ///     This method returns minimal settings without voice, animation, or noise reduction.
        ///     Use this when you want to configure all options manually.
        /// </remarks>
        public static VoiceLiveSessionOptions CreateMinimal()
        {
            return new VoiceLiveSessionOptions
            {
                Model = DefaultModel,
                Modalities = new[] { "text", "audio" },
                InputAudioFormat = DefaultInputAudioFormat,
                OutputAudioFormat = DefaultOutputAudioFormat,
                TurnDetection = new TurnDetection
                {
                    Type = "server_vad",
                    CreateResponse = true
                }
            };
        }

        #endregion
    }
}