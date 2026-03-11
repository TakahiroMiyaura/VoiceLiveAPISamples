// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Base class for Foundry Agent call status events that share common properties.
    /// </summary>
    /// <remarks>
    ///     Provides shared properties for <see cref="FoundryAgentCallInProgress" /> and
    ///     <see cref="FoundryAgentCallCompleted" />.
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    public abstract class FoundryAgentCallStatusBase : ServerEvent
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the agent response identifier.
        /// </summary>
        [JsonPropertyName("agent_response_id")]
        public string AgentResponseId { get; set; }

        /// <summary>
        ///     Gets or sets the output index.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FoundryAgentCallStatusBase" /> class.
        /// </summary>
        protected FoundryAgentCallStatusBase()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FoundryAgentCallStatusBase" /> class with specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="agentResponseId">The agent response identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        protected FoundryAgentCallStatusBase(string eventId, string itemId, string agentResponseId, int outputIndex)
        {
            EventId = eventId;
            ItemId = itemId;
            AgentResponseId = agentResponseId;
            OutputIndex = outputIndex;
        }

        #endregion
    }
}
