// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Threading.Tasks;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.InputAudioBuffers
{
    /// <summary>
    /// Provides extension methods for InputAudioBuffer operations.
    /// Contains helper methods to simplify sending audio buffer messages.
    /// </summary>
    public static class InputAudioBuffersExtension
    {
        #region Public Methods

        /// <summary>
        /// Sends the specified <see cref="InputAudioBufferAppend"/> asynchronously.
        /// This extension method provides a convenient way to send audio buffer append messages.
        /// </summary>
        /// <param name="inputAudioBufferAppend">The <see cref="InputAudioBufferAppend"/> object to send.</param>
        /// <param name="client">The <see cref="VoiceLiveAPIClientBase"/> client to send the message to.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="inputAudioBufferAppend"/> or <paramref name="client"/> is null.
        /// </exception>
        public static async Task SendAsync(this InputAudioBufferAppend inputAudioBufferAppend, VoiceLiveAPIClientBase client)
        {
            await client.SendServerAsync(inputAudioBufferAppend);
        }

        /// <summary>
        /// Sets the specified audio data to <see cref="InputAudioBufferAppend"/> and sends it asynchronously.
        /// This overload converts the byte array to base64 format before sending.
        /// </summary>
        /// <param name="inputAudioBufferAppend">The <see cref="InputAudioBufferAppend"/> object to send.</param>
        /// <param name="audioData">The audio data as a byte array to convert and send.</param>
        /// <param name="client">The <see cref="VoiceLiveAPIClientBase"/> client to send the message to.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="inputAudioBufferAppend"/>, <paramref name="audioData"/>, or <paramref name="client"/> is null.
        /// </exception>
        public static async Task SendAsync(this InputAudioBufferAppend inputAudioBufferAppend, byte[] audioData, VoiceLiveAPIClientBase client)
        {
            inputAudioBufferAppend.audio = InputAudioBufferAppend.ConvertToBase64(audioData);
            await client.SendServerAsync(inputAudioBufferAppend);
        }

        #endregion
    }
}
