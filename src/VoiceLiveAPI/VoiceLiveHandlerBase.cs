// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{

    /// <summary>
    ///     Base class for handling messages of type <typeparamref name="T" /> in the VoiceLive API.
    /// </summary>
    /// <typeparam name="T">The type of the message that this handler processes.</typeparam>
    public abstract class VoiceLiveHandlerBase<T> : IVoiceLiveHandler
    {
        /// <summary>
        ///     Gets the type of the message that this handler can process.
        /// </summary>
        public abstract string MessageType { get; }

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
        public abstract Task HandleAsync(JsonElement message);

        /// <summary>
        ///     Event triggered when a message of type <typeparamref name="T" /> is processed.
        /// </summary>
        public abstract event Action<T> OnProcessMessage;
    }
}