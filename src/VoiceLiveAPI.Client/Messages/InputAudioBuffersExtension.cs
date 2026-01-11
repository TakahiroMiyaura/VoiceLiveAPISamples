// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Messages
{
    /// <summary>
    ///     拡張メソッドを提供するクラス。
    /// </summary>
    [Obsolete(
        "This class is obsolete. Use Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages.InputAudioBuffersExtension instead.")]
    public static class InputAudioBuffersExtension
    {
        /// <summary>
        ///     指定された <see cref="InputAudioBufferAppend" /> を非同期で送信します。
        /// </summary>
        /// <param name="inputAudioBufferAppend">送信する <see cref="InputAudioBufferAppend" /> オブジェクト。</param>
        /// <param name="client">送信先の <see cref="VoiceLiveAPIClientBase" /> クライアント。</param>
        public static async Task SendAsync(this InputAudioBufferAppend inputAudioBufferAppend,
            VoiceLiveAPIClientBase client)
        {
            await client.SendServerAsync(inputAudioBufferAppend);
        }

        /// <summary>
        ///     指定された音声データを <see cref="InputAudioBufferAppend" /> に設定し、非同期で送信します。
        /// </summary>
        /// <param name="inputAudioBufferAppend">送信する <see cref="InputAudioBufferAppend" /> オブジェクト。</param>
        /// <param name="audioData">送信する音声データ。</param>
        /// <param name="client">送信先の <see cref="VoiceLiveAPIClientBase" /> クライアント。</param>
        public static async Task SendAsync(this InputAudioBufferAppend inputAudioBufferAppend, byte[] audioData,
            VoiceLiveAPIClientBase client)
        {
            inputAudioBufferAppend.Audio = InputAudioBufferAppend.ConvertToBase64(audioData);
            await client.SendServerAsync(inputAudioBufferAppend);
        }

        /// <summary>
        ///     指定された <see cref="InputAudioBufferAppend" /> を非同期で送信します。
        /// </summary>
        /// <param name="inputAudioBufferAppend">送信する <see cref="InputAudioBufferAppend" /> オブジェクト。</param>
        /// <param name="session">送信先の <see cref="VoiceLiveSession" /> セッション。</param>
        public static async Task SendAsync(this InputAudioBufferAppend inputAudioBufferAppend,
            VoiceLiveSession session)
        {
            await session.SendMessageAsync(inputAudioBufferAppend);
        }

        /// <summary>
        ///     指定された音声データを <see cref="InputAudioBufferAppend" /> に設定し、非同期で送信します。
        /// </summary>
        /// <param name="inputAudioBufferAppend">送信する <see cref="InputAudioBufferAppend" /> オブジェクト。</param>
        /// <param name="audioData">送信する音声データ。</param>
        /// <param name="session">送信先の <see cref="VoiceLiveSession" /> セッション。</param>
        public static async Task SendAsync(this InputAudioBufferAppend inputAudioBufferAppend, byte[] audioData,
            VoiceLiveSession session)
        {
            inputAudioBufferAppend.Audio = InputAudioBufferAppend.ConvertToBase64(audioData);
            await session.SendMessageAsync(inputAudioBufferAppend);
        }
    }
}