using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.InputAudioBuffers
{
    /// <summary>
    /// 拡張メソッドを提供するクラス。
    /// </summary>
    public static class InputAudioBuffersExtension
    {
        /// <summary>
        /// 指定された <see cref="InputAudioBufferAppend"/> を非同期で送信します。
        /// </summary>
        /// <param name="inputAudioBufferAppend">送信する <see cref="InputAudioBufferAppend"/> オブジェクト。</param>
        /// <param name="client">送信先の <see cref="VoiceLiveAPIClientBase"/> クライアント。</param>
        public static async Task SendAsync(this InputAudioBufferAppend inputAudioBufferAppend, VoiceLiveAPIClientBase client)
        {
            await client.SendServerAsync(inputAudioBufferAppend);
        }

        /// <summary>
        /// 指定された音声データを <see cref="InputAudioBufferAppend"/> に設定し、非同期で送信します。
        /// </summary>
        /// <param name="inputACudioBufferAppend">送信する <see cref="InputAudioBufferAppend"/> オブジェクト。</param>
        /// <param name="audioData">送信する音声データ。</param>
        /// <param name="client">送信先の <see cref="VoiceLiveAPIClientBase"/> クライアント。</param>
        public static async Task SendAsync(this InputAudioBufferAppend inputACudioBufferAppend, byte[] audioData, VoiceLiveAPIClientBase client)
        {
            inputACudioBufferAppend.audio = InputAudioBufferAppend.ConvertToBase64(audioData);
            await client.SendServerAsync(inputACudioBufferAppend);
        }
    }
}
