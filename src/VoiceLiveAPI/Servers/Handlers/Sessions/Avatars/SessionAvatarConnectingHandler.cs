using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Sessions.Avatars;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Sessions.Avatars
{
    public class SessionAvatarConnectingHandler : VoiceLiveHandlerBase<SessionAvatarConnecting>
    {
        public static string EventType = SessionAvatarConnecting.Type;

        public override string MessageType { get; } = EventType;

        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<SessionAvatarConnecting>();
            if (json == null)
                throw new InvalidOperationException("Deserialization failed for SessionAvatarConnecting.");
            json.server_sdp = Encoding.UTF8.GetString(Convert.FromBase64String(json.server_sdp));
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }

        public override event Action<SessionAvatarConnecting> OnProcessMessage;
    }
}
