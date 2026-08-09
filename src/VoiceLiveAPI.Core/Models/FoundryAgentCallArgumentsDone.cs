// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a completed hosted Foundry agent call arguments event.
    /// </summary>
    /// <remarks>
    ///     Received when the model-generated arguments for a hosted Foundry agent tool call are complete.
    ///     One of the "hosted agent invocation events" added in API version 2026-06-01-preview.
    /// </remarks>
    public class FoundryAgentCallArgumentsDone : ToolCallArgumentsDone
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type name for this event.
        /// </summary>
        public const string TypeName = "response.foundry_agent_call_arguments.done";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        #endregion
    }
}
