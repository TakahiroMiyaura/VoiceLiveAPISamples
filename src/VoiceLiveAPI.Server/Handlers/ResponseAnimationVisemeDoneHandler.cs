// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Handlers
{
    /// <summary>
    ///     Handler for processing <see cref="ResponseAnimationVisemeDone" /> messages indicating viseme animation completion.
    /// </summary>
    [Obsolete(
        "This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers.ResponseAnimationVisemeDoneHandler instead.")]
    public class ResponseAnimationVisemeDoneHandler : VoiceLiveHandlerBase<ResponseAnimationVisemeDone>
    {
        /// <summary>
        ///     The event type associated with this handler.
        /// </summary>
        public const string EventType = ResponseAnimationVisemeDone.TypeName;

        /// <inheritdoc />
        public override string MessageType => ResponseAnimationVisemeDone.TypeName;

        /// <inheritdoc />
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ResponseAnimationVisemeDoneHandler>();

        /// <inheritdoc />
        public override event Action<ResponseAnimationVisemeDone> OnProcessMessage;

        /// <inheritdoc />
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<ResponseAnimationVisemeDone>() ??
                       throw new InvalidOperationException(
                           "Deserialization failed for ResponseAnimationVisemeDoneMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}