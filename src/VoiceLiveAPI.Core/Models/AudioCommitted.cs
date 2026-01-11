// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that the input audio buffer has been committed.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>InputAudioBufferCommitted</c> class.
    /// </remarks>
    public class AudioCommitted : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "input_audio_buffer.committed";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the previous item identifier.
        /// </summary>
        [JsonPropertyName("previous_item_id")]
        public string PreviousItemId { get; set; }

        /// <summary>
        ///     Gets or sets the item identifier created from the committed audio.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AudioCommitted" /> class.
        /// </summary>
        public AudioCommitted()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AudioCommitted" /> class with specified values.
        /// </summary>
        public AudioCommitted(string eventId, string previousItemId, string itemId)
        {
            EventId = eventId;
            PreviousItemId = previousItemId;
            ItemId = itemId;
        }

        #endregion
    }
}