// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a Foundry Agent call that has failed.
    /// </summary>
    /// <remarks>
    ///     This event is fired when a Foundry Agent call encounters an error.
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    public class FoundryAgentCallFailed : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type name for this event.
        /// </summary>
        public const string TypeName = "response.foundry_agent_call.failed";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

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
        ///     Initializes a new instance of the <see cref="FoundryAgentCallFailed" /> class.
        /// </summary>
        public FoundryAgentCallFailed()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FoundryAgentCallFailed" /> class with specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        public FoundryAgentCallFailed(string eventId, string itemId, int outputIndex)
        {
            EventId = eventId;
            ItemId = itemId;
            OutputIndex = outputIndex;
        }

        #endregion
    }
}
