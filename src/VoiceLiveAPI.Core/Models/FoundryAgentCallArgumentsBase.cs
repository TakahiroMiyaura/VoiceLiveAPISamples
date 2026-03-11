// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Base class for Foundry Agent call arguments events that share common properties.
    /// </summary>
    /// <remarks>
    ///     Provides shared properties for <see cref="FoundryAgentCallArgumentsDelta" /> and
    ///     <see cref="FoundryAgentCallArgumentsDone" />.
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    public abstract class FoundryAgentCallArgumentsBase : ServerEvent
    {
        #region Properties

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

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FoundryAgentCallArgumentsBase" /> class.
        /// </summary>
        protected FoundryAgentCallArgumentsBase()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FoundryAgentCallArgumentsBase" /> class with specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        protected FoundryAgentCallArgumentsBase(string eventId, string responseId, string itemId, int outputIndex)
        {
            EventId = eventId;
            ResponseId = responseId;
            ItemId = itemId;
            OutputIndex = outputIndex;
        }

        #endregion
    }
}
