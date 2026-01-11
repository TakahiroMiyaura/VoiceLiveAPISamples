// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Factory class for creating SessionUpdate instances from raw messages.
    /// </summary>
    public static class SessionUpdateFactory
    {
        #region Static Fields

        /// <summary>
        ///     Registry of message type to factory function mappings.
        /// </summary>
        private static readonly Dictionary<string, Func<MessageBase, SessionUpdate>> Factories =
            new Dictionary<string, Func<MessageBase, SessionUpdate>>();

        #endregion

        #region Public Methods

        /// <summary>
        ///     Registers a factory function for a specific message type.
        /// </summary>
        /// <typeparam name="TMessage">The message type.</typeparam>
        /// <typeparam name="TUpdate">The SessionUpdate type.</typeparam>
        /// <param name="messageType">The message type string identifier.</param>
        /// <param name="factory">The factory function.</param>
        public static void Register<TMessage, TUpdate>(string messageType, Func<TMessage, TUpdate> factory)
            where TMessage : MessageBase
            where TUpdate : SessionUpdate
        {
            Factories[messageType] = msg => factory((TMessage)msg);
        }

        /// <summary>
        ///     Registers a factory function for a specific message type using a simpler delegate.
        /// </summary>
        /// <param name="messageType">The message type string identifier.</param>
        /// <param name="factory">The factory function.</param>
        public static void Register(string messageType, Func<MessageBase, SessionUpdate> factory)
        {
            Factories[messageType] = factory;
        }

        /// <summary>
        ///     Creates a SessionUpdate from a raw message.
        /// </summary>
        /// <param name="message">The raw message.</param>
        /// <returns>A SessionUpdate instance, or a generic SessionUpdateUnknown if the type is not registered.</returns>
        public static SessionUpdate Create(MessageBase message)
        {
            if (message == null)
            {
                return null;
            }

            if (message.Type != null && Factories.TryGetValue(message.Type, out var factory))
            {
                return factory(message);
            }

            // Return a generic unknown update for unregistered types
            return new SessionUpdateUnknown(message);
        }

        /// <summary>
        ///     Checks if a factory is registered for the specified message type.
        /// </summary>
        /// <param name="messageType">The message type string identifier.</param>
        /// <returns>True if a factory is registered, false otherwise.</returns>
        public static bool IsRegistered(string messageType)
        {
            return messageType != null && Factories.ContainsKey(messageType);
        }

        #endregion
    }
}