// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core
{
    /// <summary>
    ///     Base class for handling messages of type <typeparamref name="T" /> in the VoiceLive API.
    /// </summary>
    /// <typeparam name="T">The type of the message that this handler processes.</typeparam>
    public abstract class VoiceLiveHandlerBase<T> : IVoiceLiveHandler
    {
        #region Private Fields

        /// <summary>
        ///     Backing field for the <see cref="OnProcessMessage" /> event.
        /// </summary>
        private Action<T> _onProcessMessage;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the type of the message that this handler can process.
        /// </summary>
        public abstract string MessageType { get; }

        /// <summary>
        ///     Gets or sets the logger instance used for logging within the handler.
        /// </summary>
        public abstract ILogger Logger { set; get; }

        #endregion

        #region Events

        /// <summary>
        ///     Event triggered when a message of type <typeparamref name="T" /> is processed.
        /// </summary>
        public virtual event Action<T> OnProcessMessage
        {
            add => _onProcessMessage += value;
            remove => _onProcessMessage -= value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Determines whether the handler can process a message of the specified type.
        /// </summary>
        /// <param name="messageType">The type of the message to check.</param>
        /// <returns><c>true</c> if the handler can process the message; otherwise, <c>false</c>.</returns>
        public virtual bool CanHandle(string messageType)
        {
            return MessageType.Equals(messageType, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Handles a message represented as a <see cref="JsonElement" /> asynchronously.
        /// </summary>
        /// <param name="message">The message to handle, represented as a <see cref="JsonElement" />.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization of <typeparamref name="T" /> fails.</exception>
        public virtual Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<T>() ??
                       throw new InvalidOperationException(
                           $"Deserialization failed for {typeof(T).Name}.");
            _onProcessMessage?.Invoke(json);
            return Task.CompletedTask;
        }

        #endregion
    }
}
