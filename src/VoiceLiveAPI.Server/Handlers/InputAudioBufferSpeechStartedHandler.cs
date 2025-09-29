// Copyright (c) 2025 Takahiro Miyaura
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
    ///     Handles input audio buffer speech started messages.
    /// </summary>
    public class InputAudioBufferSpeechStartedHandler : VoiceLiveHandlerBase<InputAudioBufferSpeechStarted>
    {
        /// <summary>
        ///     Gets the event type for input audio buffer speech started.
        /// </summary>
        public const string EventType = InputAudioBufferSpeechStarted.TypeName;

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the logger instance for this handler.
        /// </summary>
        public override ILogger Logger { set; get; } =
            LoggerFactoryManager.CreateLogger<InputAudioBufferSpeechStartedHandler>();

        /// <summary>
        ///     Occurs when an input audio buffer speech started message is processed.
        /// </summary>
        public override event Action<InputAudioBufferSpeechStarted> OnProcessMessage;

        /// <summary>
        ///     Handles the input audio buffer speech started message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<InputAudioBufferSpeechStarted>() ??
                       throw new InvalidOperationException(
                           "Deserialization failed for InputAudioBufferSpeechStartedMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}