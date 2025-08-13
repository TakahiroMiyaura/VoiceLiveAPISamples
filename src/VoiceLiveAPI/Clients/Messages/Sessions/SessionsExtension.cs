using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;
using static System.Collections.Specialized.BitVector32;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Sessions
{
    /// <summary>
    /// 拡張メソッドを提供するクラス。
    /// </summary>
    public static class SessionsExtension
    {
        /// <summary>
        /// クライアントセッション更新メッセージを送信します。
        /// </summary>
        /// <param name="clientSessionUpdated">送信するクライアントセッション更新メッセージ。</param>
        /// <param name="client">送信先の VoiceLiveAPI クライアント。</param>
        public static async Task SendAsync(this ClientSessionUpdate clientSessionUpdated, VoiceLiveAPIClientBase client)
        {
            await client.SendServerAsync(clientSessionUpdated);
        }
    }
}
