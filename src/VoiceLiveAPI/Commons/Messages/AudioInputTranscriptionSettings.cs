// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>  
    /// Represents the settings for audio input transcription.  
    /// </summary>  
    public class AudioInputTranscriptionSettings
    {
        /// <summary>  
        /// Gets or sets the list of phrases to assist the transcription model.  
        /// </summary>  
        public string[] phrase_list = null;

        /// <summary>  
        /// Gets or sets the transcription model to be used.  
        /// Supported values: "whisper-1", "gpt-4o-transcribe", "gpt-4o-mini-transcribe".  
        /// </summary>  
        public string model { get; set; } = null;

        /// <summary>  
        /// Gets or sets a value indicating whether a custom model is used.  
        /// </summary>  
        public bool? custom_model { get; set; }
    }
}