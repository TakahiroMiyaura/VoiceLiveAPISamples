// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Messages.Unconfirmed.OutputAudioBuffers;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Handlers.OutputAudioBuffers
{

    /// <summary>
    ///     Handles output audio buffer cleared messages.
    /// </summary>
    public class OutputAudioBufferClearedHandler : VoiceLiveHandlerBase<OutputAudioBufferClearedMessage>
    {
        /// <summary>
        ///     Gets the event type for output audio buffer cleared.
        /// </summary>
        public static string EventType = "output_audio_buffer.cleared";

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Occurs when an output audio buffer cleared message is processed.
        /// </summary>
        public override event Action<OutputAudioBufferClearedMessage> OnProcessMessage = null;

        /// <summary>
        ///     Handles the output audio buffer cleared message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var messageString = message.GetRawText();
            var outputAudioBufferCleared = JsonSerializer.Deserialize<OutputAudioBufferClearedMessage>(messageString);
            using (var writer =
                   new StreamWriter(new FileStream(EventType + ".txt", FileMode.Create)))
            {
                await writer.WriteAsync(messageString);
                await writer.FlushAsync();
            }

            if (outputAudioBufferCleared == null)
                throw new InvalidOperationException("Deserialization failed for OutputAudioBufferClearedMessage.");
            OnProcessMessage?.Invoke(outputAudioBufferCleared);
            await Task.CompletedTask;
        }
    }
}