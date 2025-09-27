using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Sessions.Avatars
{
    /// <summary>
    /// 拡張メソッドを提供するクラス。
    /// </summary>
    public static class SessionAvatarExtension
    {
        /// <summary>
        /// 指定された <see cref="SessionAvatarConnect"/> を非同期で送信します。
        /// </summary>
        /// <param name="sessionAvatarConnect">送信する <see cref="SessionAvatarConnect"/> オブジェクト。</param>
        /// <param name="client">送信先の <see cref="VoiceLiveAPIClientBase"/> クライアント。</param>
        public static async Task SendAsync(this SessionAvatarConnect sessionAvatarConnect, VoiceLiveAPIClientBase client)
        {
            await client.SendServerAsync(sessionAvatarConnect);
        }
    }
}
