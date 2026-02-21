// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified
{
    /// <summary>
    ///     Represents information about a content part, which can include text, audio, images, or references.
    /// </summary>
    /// <remarks>
    ///     Supported content types:
    ///     <list type="bullet">
    ///         <item>
    ///             <term>input_text</term>
    ///             <description>Text input from the user.</description>
    ///         </item>
    ///         <item>
    ///             <term>input_audio</term>
    ///             <description>Audio input (base64-encoded).</description>
    ///         </item>
    ///         <item>
    ///             <term>input_image</term>
    ///             <description>Image input (base64-encoded or URL).</description>
    ///         </item>
    ///         <item>
    ///             <term>item_reference</term>
    ///             <description>Reference to another item.</description>
    ///         </item>
    ///         <item>
    ///             <term>text</term>
    ///             <description>Text content in response.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public class ContentPartInfo
    {
        /// <summary>
        ///     Gets or sets the type of the content part.
        ///     Allowed values: input_text, input_audio, input_image, item_reference, text.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the text content of the content part.
        ///     Used when Type is "input_text" or "text".
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = null;

        /// <summary>
        ///     Gets or sets the unique identifier of the content part.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null;

        /// <summary>
        ///     Gets or sets the audio content of the content part (base64-encoded).
        ///     Used when Type is "input_audio".
        /// </summary>
        [JsonPropertyName("audio")]
        public string Audio { get; set; } = null;

        /// <summary>
        ///     Gets or sets the transcript of the audio content.
        /// </summary>
        [JsonPropertyName("transcript")]
        public string Transcript { get; set; } = null;

        /// <summary>
        ///     Gets or sets the image URL or base64 data URI.
        ///     Used when Type is "input_image".
        ///     Format: "data:image/{format};base64,{base64_encoded_data}" or a URL.
        /// </summary>
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = null;
    }
}