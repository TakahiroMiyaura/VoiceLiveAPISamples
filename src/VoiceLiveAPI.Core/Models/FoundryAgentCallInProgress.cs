// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a hosted Foundry agent call in-progress event.
    /// </summary>
    /// <remarks>
    ///     Received when a hosted Foundry agent invoked as a tool starts processing.
    ///     One of the "hosted agent invocation events" added in API version 2026-06-01-preview
    ///     (the underlying <c>response.foundry_agent_call.*</c> events exist from 2026-01-01-preview).
    /// </remarks>
    public class FoundryAgentCallInProgress : ToolCallStatus
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
    }
}
