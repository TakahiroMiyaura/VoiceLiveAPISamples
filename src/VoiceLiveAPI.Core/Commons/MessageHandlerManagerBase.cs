// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Collections.Generic;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons
{
    /// <summary>
    ///     Abstract base class for managing message handlers.
    ///     Provides registration and retrieval of message handlers.
    /// </summary>
    public abstract class MessageHandlerManagerBase : ILogOutputClass
    {
        /// <summary>
        ///     Dictionary for storing registered message handlers.
        /// </summary>
        private readonly Dictionary<string, IVoiceLiveHandler> messageHandlers =
            new Dictionary<string, IVoiceLiveHandler>();

        /// <summary>
        ///     Gets or sets the logger for outputting logs.
        /// </summary>
        public abstract ILogger Logger { set; get; }

        /// <summary>
        ///     Registers a message handler.
        /// </summary>
        /// <param name="handler">The message handler to register.</param>
        public void RegisterMessageHandler(IVoiceLiveHandler handler)
        {
            messageHandlers[handler.MessageType] = handler;
            Logger.Log(LogLevel.Debug, "Registered handler for message type: {handler.MessageType}",
                handler.MessageType);
        }

        /// <summary>
        ///     Gets the list of registered message handlers.
        /// </summary>
        /// <returns>A dictionary of registered handlers.</returns>
        public IReadOnlyDictionary<string, IVoiceLiveHandler> GetRegisteredHandlers()
        {
            return messageHandlers;
        }

        /// <summary>
        ///     Tries to get the message handler for the specified key.
        /// </summary>
        /// <param name="key">The message type key.</param>
        /// <param name="handler">The retrieved handler.</param>
        /// <returns>True if the handler exists; otherwise, false.</returns>
        public bool TryGetValue(string key, out IVoiceLiveHandler handler)
        {
            return messageHandlers.TryGetValue(key, out handler);
        }
    }
}