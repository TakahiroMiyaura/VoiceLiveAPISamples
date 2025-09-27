// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Sessions.Avatars;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Sessions.Avatars
{
    /// <summary>
    /// Handles session avatar connecting messages for WebRTC avatar video streaming setup.
    /// </summary>
    public class SessionAvatarConnectingHandler : VoiceLiveHandlerBase<SessionAvatarConnecting>
    {
        #region Static Fields and Constants

        /// <summary>
        /// Gets the event type for session avatar connecting messages.
        /// </summary>
        public static string EventType = SessionAvatarConnecting.Type;

        #endregion

        #region Events

        /// <summary>
        /// Event fired when a session avatar connecting message is processed.
        /// </summary>
        public override event Action<SessionAvatarConnecting> OnProcessMessage;

        #endregion

        #region Properties, Indexers

        /// <summary>
        /// Gets the message type handled by this handler.
        /// </summary>
        public override string MessageType { get; } = EventType;

        #endregion

        #region Public methods

        /// <summary>
        /// Handles the session avatar connecting message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message element to handle.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<SessionAvatarConnecting>();
            if (json == null)
                throw new InvalidOperationException("Deserialization failed for SessionAvatarConnecting.");
            json.server_sdp = Encoding.UTF8.GetString(Convert.FromBase64String(json.server_sdp));
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }

        #endregion
    }
}
