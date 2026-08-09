// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers
{
    /// <summary>
    ///     Generic handler for server events that follow the standard deserialization pattern.
    /// </summary>
    /// <typeparam name="T">The server event model type to deserialize and dispatch.</typeparam>
    /// <remarks>
    ///     Eliminates the need for individual handler classes when the handling logic
    ///     is simply deserialize-and-invoke. Each instance is bound to a specific event type string.
    /// </remarks>
    public class GenericServerEventHandler<T> : VoiceLiveHandlerBase<T> where T : ServerEvent
    {
        #region Private Fields

        /// <summary>
        ///     The event type string this handler is registered for.
        /// </summary>
        private readonly string eventType;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the message type handled by this handler.
        /// </summary>
        public override string MessageType => eventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<GenericServerEventHandler<T>>();

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a message of type <typeparamref name="T" /> is processed.
        /// </summary>
        public override event Action<T> OnProcessMessage;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GenericServerEventHandler{T}" /> class.
        /// </summary>
        /// <param name="eventType">The event type string this handler responds to.</param>
        public GenericServerEventHandler(string eventType)
        {
            this.eventType = eventType;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Handles the incoming message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public override Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<T>() ??
                       throw new InvalidOperationException(
                           $"Deserialization failed for {typeof(T).Name}.");
            OnProcessMessage?.Invoke(json);
            return Task.CompletedTask;
        }

        #endregion
    }
}
