// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses.Animations;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.Responses.Animations
{
    public class ResponseAnimationVisemeDoneHandler : VoiceLiveHandlerBase<ResponseAnimationVisemeDone>
    {
        /// <summary>
        ///     The event type associated with this handler.
        /// </summary>
        public static string EventType = ResponseAnimationVisemeDone.Type;

        public override string MessageType => ResponseAnimationVisemeDone.Type;

        public override event Action<ResponseAnimationVisemeDone> OnProcessMessage;

        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ResponseAnimationVisemeDone>();
            if (json == null)
                throw new InvalidOperationException("Deserialization failed for ResponseAnimationVisemeDoneMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}