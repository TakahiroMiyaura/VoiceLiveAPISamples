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
    /// Handles input audio buffer speech stopped messages.
    /// This handler processes notifications when speech ends in the audio input buffer.
    /// </summary>
    public class InputAudioBufferSpeechStoppedHandler : VoiceLiveHandlerBase<InputAudioBufferSpeechStopped>
    {
        #region Static Fields

        /// <summary>
        /// Gets the event type for input audio buffer speech stopped.
        /// </summary>
        public static string EventType = InputAudioBufferSpeechStopped.Type;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an input audio buffer speech stopped message is processed.
        /// Subscribe to this event to receive notifications when speech detection ends.
        /// </summary>
        public override event Action<InputAudioBufferSpeechStopped> OnProcessMessage = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the message type this handler can process.
        /// </summary>
        /// <value>
        /// The message type identifier for speech stopped events.
        /// </value>
        public override string MessageType => EventType;

        #endregion

        #region Public Methods

        /// <summary>
        /// Handles the input audio buffer speech stopped message asynchronously.
        /// Deserializes the message and invokes the processing event.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when deserialization of the message fails.
        /// </exception>
        public override async Task HandleAsync(JsonElement message)
        {
            var json = message.Deserialize<InputAudioBufferSpeechStopped>();
            if (json == null)
                throw new InvalidOperationException("Deserialization failed for InputAudioBufferSpeechStoppedMessage.");
            
            OnProcessMessage?.Invoke(json);
            await Task.CompletedTask;
        }

        #endregion
    }
}