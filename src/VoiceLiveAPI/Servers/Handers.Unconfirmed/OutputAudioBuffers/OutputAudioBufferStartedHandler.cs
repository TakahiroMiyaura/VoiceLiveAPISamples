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
    ///     Handles output audio buffer started messages.
    /// </summary>
    public class OutputAudioBufferStartedHandler : VoiceLiveHandlerBase<OutputAudioBufferStartedMessage>
    {
        /// <summary>
        ///     Gets the event type for output audio buffer started.
        /// </summary>
        public static string EventType = "output_audio_buffer.started";

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Occurs when an output audio buffer started message is processed.
        /// </summary>
        public override event Action<OutputAudioBufferStartedMessage> OnProcessMessage = null;

        /// <summary>
        ///     Handles the output audio buffer started message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var messageString = message.GetRawText();
            var outputAudioBufferStarted = JsonSerializer.Deserialize<OutputAudioBufferStartedMessage>(messageString);
            using (var writer =
                   new StreamWriter(new FileStream(EventType + ".txt", FileMode.Create)))
            {
                await writer.WriteAsync(messageString);
                await writer.FlushAsync();
            }

            if (outputAudioBufferStarted == null)
                throw new InvalidOperationException("Deserialization failed for OutputAudioBufferStartedMessage.");
            OnProcessMessage?.Invoke(outputAudioBufferStarted);
            await Task.CompletedTask;
        }
    }
}