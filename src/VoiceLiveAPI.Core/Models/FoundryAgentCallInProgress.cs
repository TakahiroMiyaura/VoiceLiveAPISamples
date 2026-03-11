// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a Foundry Agent call that has started processing.
    /// </summary>
    /// <remarks>
    ///     This event is fired when a Foundry Agent begins processing a call.
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    public class FoundryAgentCallInProgress : FoundryAgentCallStatusBase
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type name for this event.
        /// </summary>
        public const string TypeName = "response.foundry_agent_call.in_progress";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FoundryAgentCallInProgress" /> class.
        /// </summary>
        public FoundryAgentCallInProgress()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FoundryAgentCallInProgress" /> class with specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="agentResponseId">The agent response identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        public FoundryAgentCallInProgress(string eventId, string itemId, string agentResponseId, int outputIndex)
            : base(eventId, itemId, agentResponseId, outputIndex)
        {
        }

        #endregion
    }
}
