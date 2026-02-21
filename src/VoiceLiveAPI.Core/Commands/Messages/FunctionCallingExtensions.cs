// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages
{
    /// <summary>
    ///     Function Calling 関連の拡張メソッドを提供するクラス。
    /// </summary>
    public static class FunctionCallingExtensions
    {
        #region Public Methods

        /// <summary>
        ///     Function call の結果を送信します。
        /// </summary>
        /// <param name="session">送信先の <see cref="VoiceLiveSession" /> セッション。</param>
        /// <param name="callId">Function call の call_id。</param>
        /// <param name="output">関数の実行結果（JSON文字列）。</param>
        /// <param name="itemId">オプションのアイテムID。指定しない場合はサーバーが生成します。</param>
        /// <returns>送信が完了したことを示すタスク。</returns>
        public static async Task SendFunctionCallOutputAsync(
            this VoiceLiveSession session,
            string callId,
            string output,
            string itemId = null)
        {
            if (string.IsNullOrEmpty(callId))
            {
                throw new ArgumentNullException(nameof(callId), "call_id is required for function_call_output.");
            }

            var message = new ConversationItemCreateMessage
            {
                Item = new ConversationRequestItem
                {
                    Type = "function_call_output",
                    Id = itemId,
                    CallId = callId,
                    Output = output
                }
            };
            await session.SendMessageAsync(message);
        }

        /// <summary>
        ///     Function call の結果をオブジェクトとして送信します。
        ///     オブジェクトは自動的にJSONにシリアライズされます。
        /// </summary>
        /// <typeparam name="T">出力オブジェクトの型。</typeparam>
        /// <param name="session">送信先の <see cref="VoiceLiveSession" /> セッション。</param>
        /// <param name="callId">Function call の call_id。</param>
        /// <param name="outputObject">関数の実行結果オブジェクト。</param>
        /// <param name="itemId">オプションのアイテムID。指定しない場合はサーバーが生成します。</param>
        /// <returns>送信が完了したことを示すタスク。</returns>
        public static async Task SendFunctionCallOutputAsync<T>(
            this VoiceLiveSession session,
            string callId,
            T outputObject,
            string itemId = null)
        {
            var output = JsonSerializer.Serialize(outputObject);
            await session.SendFunctionCallOutputAsync(callId, output, itemId);
        }

        /// <summary>
        ///     <see cref="FunctionCallDone" /> イベントに対して Function call の結果を送信します。
        /// </summary>
        /// <param name="functionCallDone">受信した <see cref="FunctionCallDone" /> イベント。</param>
        /// <param name="session">送信先の <see cref="VoiceLiveSession" /> セッション。</param>
        /// <param name="output">関数の実行結果（JSON文字列）。</param>
        /// <returns>送信が完了したことを示すタスク。</returns>
        public static async Task RespondAsync(
            this FunctionCallDone functionCallDone,
            VoiceLiveSession session,
            string output)
        {
            await session.SendFunctionCallOutputAsync(functionCallDone.CallId, output);
        }

        /// <summary>
        ///     <see cref="FunctionCallDone" /> イベントに対して Function call の結果をオブジェクトとして送信します。
        ///     オブジェクトは自動的にJSONにシリアライズされます。
        /// </summary>
        /// <typeparam name="T">出力オブジェクトの型。</typeparam>
        /// <param name="functionCallDone">受信した <see cref="FunctionCallDone" /> イベント。</param>
        /// <param name="session">送信先の <see cref="VoiceLiveSession" /> セッション。</param>
        /// <param name="outputObject">関数の実行結果オブジェクト。</param>
        /// <returns>送信が完了したことを示すタスク。</returns>
        public static async Task RespondAsync<T>(
            this FunctionCallDone functionCallDone,
            VoiceLiveSession session,
            T outputObject)
        {
            var output = JsonSerializer.Serialize(outputObject);
            await session.SendFunctionCallOutputAsync(functionCallDone.CallId, output);
        }

        /// <summary>
        ///     Function call の引数をデシリアライズします。
        /// </summary>
        /// <typeparam name="T">引数の型。</typeparam>
        /// <param name="functionCallDone">受信した <see cref="FunctionCallDone" /> イベント。</param>
        /// <returns>デシリアライズされた引数オブジェクト。</returns>
        public static T GetArguments<T>(this FunctionCallDone functionCallDone)
        {
            if (string.IsNullOrEmpty(functionCallDone.Arguments))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(functionCallDone.Arguments);
        }

        #endregion
    }
}
