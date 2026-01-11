// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a notification that a conversation item has been truncated.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ConversationItemTruncatedMessage</c> class.
    /// </remarks>
    public class ItemTruncated : ServerEvent
    {
        #region Properties

        /// <inheritdoc />
        public override string Type => "conversation.item.truncated";

        /// <summary>
        ///     Gets or sets the truncated item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the content index where truncation occurred.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the audio end time in milliseconds.
        /// </summary>
        [JsonPropertyName("audio_end_ms")]
        public int AudioEndMs { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemTruncated" /> class.
        /// </summary>
        public ItemTruncated()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemTruncated" /> class with specified values.
        /// </summary>
        public ItemTruncated(string eventId, string itemId, int contentIndex, int audioEndMs)
        {
            EventId = eventId;
            ItemId = itemId;
            ContentIndex = contentIndex;
            AudioEndMs = audioEndMs;
        }

        #endregion
    }
}