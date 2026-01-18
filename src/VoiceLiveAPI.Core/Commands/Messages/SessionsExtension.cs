// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Threading.Tasks;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages
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
        /// <param name="session">送信先の <see cref="VoiceLiveSession" /> セッション。</param>
        public static async Task SendAsync(this ClientSessionUpdate clientSessionUpdated, VoiceLiveSession session)
        {
            await session.SendMessageAsync(clientSessionUpdated);
        }
    }
}