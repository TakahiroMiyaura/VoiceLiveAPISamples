// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Represents an item with various properties such as ID, type, status, role, and content.
    /// </summary>
    public class Item
    {
        /// <summary>
        ///     Gets or sets the unique identifier of the item.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null;

        /// <summary>
        ///     Gets or sets the type of the item.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the status of the item.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = null;

        /// <summary>
        ///     Gets or sets the role associated with the item.
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = null;

        /// <summary>
        ///     Gets or sets the content associated with the item.
        /// </summary>
        [JsonPropertyName("content")]
        public Content[] Content { get; set; } = null;
    }
}