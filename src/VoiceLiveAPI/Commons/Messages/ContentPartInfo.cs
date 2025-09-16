// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages
{

    /// <summary>
    /// Represents information about a content part, which can include text, audio, or references.
    /// </summary>
    public class ContentPartInfo
    {
        /// <summary>
        /// Gets or sets the type of the content part.
        /// Allowed values: input_text, input_audio, item_reference, text.
        /// </summary>
        public string type { get; set; } = null;

        /// <summary>
        /// Gets or sets the text content of the content part.
        /// </summary>
        public string text { get; set; } = null;

        /// <summary>
        /// Gets or sets the unique identifier of the content part.
        /// </summary>
        public string id { get; set; } = null;

        /// <summary>
        /// Gets or sets the audio content of the content part.
        /// </summary>
        public string audio { get; set; } = null;

        /// <summary>
        /// Gets or sets the transcript of the audio content.
        /// </summary>
        public string transcript { get; set; } = null;
    }
}