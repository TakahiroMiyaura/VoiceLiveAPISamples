// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using System.Threading.Tasks;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{

    /// <summary>
    ///     Interface for handling VoiceInfo Live API messages.
    /// </summary>
    public interface IVoiceLiveHandler
    {
        /// <summary>
        ///     Gets the type of message this handler can process.
        /// </summary>
        string MessageType { get; }

        /// <summary>
        ///     Determines whether this handler can process a specific message type.
        /// </summary>
        /// <param name="messageType">The type of the message to check.</param>
        /// <returns>True if the handler can process the message; otherwise, false.</returns>
        bool CanHandle(string messageType);

        /// <summary>
        ///     Processes a message asynchronously.
        /// </summary>
        /// <param name="message">The JSON element representing the message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleAsync(JsonElement message);
    }
}