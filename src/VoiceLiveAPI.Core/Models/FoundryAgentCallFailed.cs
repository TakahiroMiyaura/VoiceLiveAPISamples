// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a hosted Foundry agent call failed event.
    /// </summary>
    /// <remarks>
    ///     Received when a hosted Foundry agent invoked as a tool fails.
    ///     One of the "hosted agent invocation events" added in API version 2026-06-01-preview.
    /// </remarks>
    public class FoundryAgentCallFailed : ToolCallStatus
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

        #endregion
    }
}
