// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars.Servers.Message;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Handlers;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server
{
    /// <summary>
    ///     Manages message handlers specifically for avatar-related WebRTC communication.
    ///     Provides event-driven handling for avatar session messages with lazy registration.
    /// </summary>
    [Obsolete("This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.AvatarMessageHandlerManager instead.")]
    public class AvatarMessageHandlerManager : MessageHandlerManagerBase
    {
        #region Properties, Indexers

        /// <summary>
        ///     Gets or sets the logger instance for avatar message handler operations.
        /// </summary>
        public override ILogger Logger { set; get; } = LoggerFactoryManager.CreateLogger<AvatarMessageHandlerManager>();

        #endregion

        #region Events

        /// <summary>
        ///     Event fired when a session avatar connecting message is processed.
        ///     Handles WebRTC peer connection establishment for avatar video streaming.
        /// </summary>
        public event Action<SessionAvatarConnecting> OnSessionAvatarConnecting
        {
            add
            {
                if (TryGetValue(SessionAvatarConnectingHandler.EventType, out var handler))
                {
                    ((SessionAvatarConnectingHandler)handler).OnProcessMessage += value;
                }
                else
                {
                    var h = new SessionAvatarConnectingHandler();
                    h.OnProcessMessage += value;
                    RegisterMessageHandler(h);
                }
            }
            remove
            {
                if (TryGetValue(SessionAvatarConnectingHandler.EventType, out var handler))
                {
                    ((SessionAvatarConnectingHandler)handler).OnProcessMessage -= value;
                }
                else
                {
                    throw new InvalidOperationException("Handler not registered for SessionAvatarConnectingHandler.");
                }
            }
        }

        #endregion
    }
}