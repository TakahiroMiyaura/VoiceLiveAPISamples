// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a viseme animation delta for avatar lip-sync.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ResponseAnimationVisemeDelta</c> class.
    /// </remarks>
    public class VisemeDelta : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "response.animation_viseme.delta";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the response identifier.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

        /// <summary>
        ///     Gets or sets the item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the output index.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the content index.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the audio offset in milliseconds.
        /// </summary>
        [JsonPropertyName("audio_offset_ms")]
        public int? AudioOffsetMs { get; set; }

        /// <summary>
        ///     Gets or sets the viseme identifier.
        /// </summary>
        [JsonPropertyName("viseme_id")]
        public int? VisemeId { get; set; }

        /// <summary>
        ///     Gets or sets the viseme data (alternative format).
        /// </summary>
        [JsonPropertyName("viseme")]
        public object Viseme { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="VisemeDelta" /> class.
        /// </summary>
        public VisemeDelta()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VisemeDelta" /> class with specified values.
        /// </summary>
        public VisemeDelta(string eventId, string responseId, string itemId, int outputIndex, int contentIndex,
            int? audioOffsetMs = null, int? visemeId = null, object viseme = null)
        {
            EventId = eventId;
            ResponseId = responseId;
            ItemId = itemId;
            OutputIndex = outputIndex;
            ContentIndex = contentIndex;
            AudioOffsetMs = audioOffsetMs;
            VisemeId = visemeId;
            Viseme = viseme;
        }

        #endregion
    }
}