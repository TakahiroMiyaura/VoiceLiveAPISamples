// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.InputAudioBuffers;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.InputAudioBuffers
{

    /// <summary>
    ///     Handles input audio buffer speech stopped messages.
    /// </summary>
    public class InputAudioBufferSpeechStoppedHandler : VoiceLiveHandlerBase<InputAudioBufferSpeechStopped>
    {
        /// <summary>
        ///     Gets the event type for input audio buffer speech stopped.
        /// </summary>
        public static string EventType = InputAudioBufferSpeechStopped.Type;

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Occurs when an input audio buffer speech stopped message is processed.
        /// </summary>
        public override event Action<InputAudioBufferSpeechStopped> OnProcessMessage = null;

        /// <summary>
        ///     Handles the input audio buffer speech stopped message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<InputAudioBufferSpeechStopped>();
            if (json == null)
                throw new InvalidOperationException("Deserialization failed for InputAudioBufferSpeechStoppedMessage.");
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }
    }
}