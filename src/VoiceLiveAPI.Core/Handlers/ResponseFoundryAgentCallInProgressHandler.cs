// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers
{
    /// <summary>
    ///     Handles messages of type "response.foundry_agent_call.in_progress".
    /// </summary>
    public class ResponseFoundryAgentCallInProgressHandler : VoiceLiveHandlerBase<FoundryAgentCallInProgress>
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The event type associated with this handler.
        /// </summary>
        public const string EventType = FoundryAgentCallInProgress.TypeName;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the message type handled by this handler.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ResponseFoundryAgentCallInProgressHandler>();

        #endregion
    }
}
