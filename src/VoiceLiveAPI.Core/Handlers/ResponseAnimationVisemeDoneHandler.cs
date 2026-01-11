// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Handlers
{
    /// <summary>
    ///     Handler for processing <see cref="VisemeDone" /> messages indicating viseme animation completion.
    /// </summary>
    public class ResponseAnimationVisemeDoneHandler : VoiceLiveHandlerBase<VisemeDone>
    {
        /// <summary>
        ///     The event type associated with this handler.
        /// </summary>
        public const string EventType = VisemeDone.TypeName;

        /// <inheritdoc />
        public override string MessageType => VisemeDone.TypeName;

        /// <inheritdoc />
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<ResponseAnimationVisemeDoneHandler>();

        /// <inheritdoc />
        public override event Action<VisemeDone> OnProcessMessage;

        /// <inheritdoc />
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<VisemeDone>() ??
                       throw new InvalidOperationException(
                           "Deserialization failed for VisemeDone.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}