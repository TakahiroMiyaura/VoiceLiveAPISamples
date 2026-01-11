// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars.Clients.Messages
{
    /// <summary>
    ///     拡張メソッドを提供するクラス。
    /// </summary>
    [Obsolete(
        "This class is obsolete. Please use the Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages.SessionAvatarExtension",
        true)]
    public static class SessionAvatarExtension
    {
        /// <summary>
        ///     指定された <see cref="SessionAvatarConnect" /> を非同期で送信します。
        /// </summary>
        /// <param name="sessionAvatarConnect">送信する <see cref="SessionAvatarConnect" /> オブジェクト。</param>
        /// <param name="client">送信先の <see cref="VoiceLiveAPIClientBase" /> クライアント。</param>
        public static async Task SendAsync(this SessionAvatarConnect sessionAvatarConnect,
            VoiceLiveAPIClientBase client)
        {
            await client.SendServerAsync(sessionAvatarConnect);
        }
    }
}