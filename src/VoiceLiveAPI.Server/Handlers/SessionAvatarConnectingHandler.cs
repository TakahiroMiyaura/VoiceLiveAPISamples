// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars.Servers.Message;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Handlers
{
    /// <summary>
    ///     Handles the processing of SessionAvatarConnecting messages for WebRTC avatar setup.
    ///     Decodes the server SDP and triggers the OnProcessMessage event.
    /// </summary>
    [Obsolete(
        "This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers.SessionAvatarConnectingHandler instead.")]
    public class SessionAvatarConnectingHandler : VoiceLiveHandlerBase<SessionAvatarConnecting>
    {
        /// <summary>
        ///     The event type name for session avatar connecting.
        /// </summary>
        public const string EventType = SessionAvatarConnecting.TypeName;

        /// <summary>
        ///     Gets the message type handled by this handler.
        /// </summary>
        public override string MessageType { get; } = EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<SessionAvatarConnectingHandler>();

        /// <summary>
        ///     Handles the incoming SessionAvatarConnecting message asynchronously.
        ///     Decodes the ServerSdp property and invokes the OnProcessMessage event.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<SessionAvatarConnecting>() ??
                       throw new InvalidOperationException("Deserialization failed for SessionAvatarConnecting.");
            json.ServerSdp = Encoding.UTF8.GetString(Convert.FromBase64String(json.ServerSdp));
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }

        /// <summary>
        ///     Occurs when a SessionAvatarConnecting message is processed.
        /// </summary>
        public override event Action<SessionAvatarConnecting> OnProcessMessage;
    }
}