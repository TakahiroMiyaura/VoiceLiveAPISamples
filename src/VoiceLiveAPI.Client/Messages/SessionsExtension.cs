// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Messages
{
    /// <summary>
    ///     拡張メソッドを提供するクラス。
    /// </summary>
    public static class SessionsExtension
    {
        /// <summary>
        ///     クライアントセッション更新メッセージを送信します。
        /// </summary>
        /// <param name="clientSessionUpdated">送信するクライアントセッション更新メッセージ。</param>
        /// <param name="client">送信先の VoiceLiveAPI クライアント。</param>
        public static async Task SendAsync(this ClientSessionUpdate clientSessionUpdated, VoiceLiveAPIClientBase client)
        {
            await client.SendServerAsync(clientSessionUpdated);
        }
    }
}