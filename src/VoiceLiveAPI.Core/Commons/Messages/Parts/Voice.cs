// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents a voice configuration with various properties such as name, type, and customization options.
    /// </summary>
    public class Voice
    {
        /// <summary>
        ///     Gets or sets the name of the voice.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null;

        /// <summary>
        ///     Gets or sets the type of the voice.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the temperature value for the voice configuration.
        /// </summary>
        [JsonPropertyName("temperature")]
        public object Temperature { get; set; } = null;

        /// <summary>
        ///     Gets or sets the URL for a custom lexicon.
        /// </summary>
        [JsonPropertyName("custom_lexicon_url")]
        public object CustomLexiconUrl { get; set; } = null;

        /// <summary>
        ///     Gets or sets the preferred locales for the voice.
        /// </summary>
        [JsonPropertyName("prefer_locales")]
        public object PreferLocales { get; set; } = null;

        /// <summary>
        ///     Gets or sets the style of the voice.
        /// </summary>
        [JsonPropertyName("style")]
        public object Style { get; set; } = null;

        /// <summary>
        ///     Gets or sets the pitch of the voice.
        /// </summary>
        [JsonPropertyName("pitch")]
        public object Pitch { get; set; } = null;

        /// <summary>
        ///     Gets or sets the rate of speech for the voice.
        /// </summary>
        [JsonPropertyName("rate")]
        public object Rate { get; set; } = null;

        /// <summary>
        ///     Gets or sets the volume of the voice.
        /// </summary>
        [JsonPropertyName("volume")]
        public object Volume { get; set; } = null;
    }
}