// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.InputAudioBuffers;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handers.Unconfirmed.InputAudioBuffers
{

    /// <summary>
    ///     Handles input audio buffer cleared messages.
    /// </summary>
    public class InputAudioBufferClearedHandler : VoiceLiveHandlerBase<InputAudioBufferClearedMessage>
    {
        /// <summary>
        ///     Gets the event type for input audio buffer cleared.
        /// </summary>
        public static string EventType = "input_audio_buffer.cleared";

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Occurs when an input audio buffer cleared message is processed.
        /// </summary>
        public override event Action<InputAudioBufferClearedMessage> OnProcessMessage = null;

        /// <summary>
        ///     Handles the input audio buffer cleared message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var messageString = message.GetRawText();
            var inputAudioBufferCleared = JsonSerializer.Deserialize<InputAudioBufferClearedMessage>(messageString);
            using (var writer =
                   new StreamWriter(new FileStream(EventType + ".txt", FileMode.Create)))
            {
                await writer.WriteAsync(messageString);
                await writer.FlushAsync();
            }

            if (inputAudioBufferCleared == null)
                throw new InvalidOperationException("Deserialization failed for InputAudioBufferClearedMessage.");
            OnProcessMessage?.Invoke(inputAudioBufferCleared);
            await Task.CompletedTask;
        }
    }
}