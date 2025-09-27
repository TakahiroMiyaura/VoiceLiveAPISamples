// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Sessions
{
    /// <summary>
    /// Provides extension methods for session operations.
    /// Contains helper methods to simplify sending session-related messages.
    /// </summary>
    public static class SessionsExtension
    {
        #region Public Methods

        /// <summary>
        /// Sends a client session update message asynchronously.
        /// This extension method provides a convenient way to send session updates.
        /// </summary>
        /// <param name="clientSessionUpdated">The client session update message to send.</param>
        /// <param name="client">The VoiceLiveAPI client to send the message to.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="clientSessionUpdated"/> or <paramref name="client"/> is null.
        /// </exception>
        public static async Task SendAsync(this ClientSessionUpdate clientSessionUpdated, VoiceLiveAPIClientBase client)
        {
            await client.SendServerAsync(clientSessionUpdated);
        }

        #endregion
    }
}
