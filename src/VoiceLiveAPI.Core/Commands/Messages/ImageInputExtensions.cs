// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages
{
    /// <summary>
    ///     画像入力関連の拡張メソッドを提供するクラス。
    /// </summary>
    /// <remarks>
    ///     OpenAI Realtime API の conversation.item.create メッセージを使用して、
    ///     画像データをセッションに送信します。
    ///     画像は base64 データ URI（data:image/{format};base64,...）
    ///     または HTTPS URL として指定できます。
    /// </remarks>
    public static class ImageInputExtensions
    {
        #region Static Fields and Constants

        /// <summary>
        ///     サポートされる画像フォーマットの MIME タイプマッピング。
        /// </summary>
        private static readonly Dictionary<string, string> SupportedImageFormats = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".webp", "image/webp" }
        };

        #endregion

        #region Public Methods

        /// <summary>
        ///     画像を送信します。
        /// </summary>
        /// <param name="session">送信先の <see cref="VoiceLiveSession" /> セッション。</param>
        /// <param name="imageUrl">
        ///     画像の URL またはBase64データURI。
        ///     形式: "data:image/{format};base64,{base64_data}" または HTTPS URL。
        /// </param>
        /// <param name="cancellationToken">キャンセレーショントークン。</param>
        /// <returns>送信が完了したことを示すタスク。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="imageUrl" /> が null または空の場合。</exception>
        public static async Task SendImageAsync(
            this VoiceLiveSession session,
            string imageUrl,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentNullException(nameof(imageUrl), "Image URL or data URI is required.");
            }

            var message = new ConversationItemCreateMessage
            {
                Item = new ConversationRequestItem
                {
                    Type = "message",
                    Role = "user",
                    Content = new[]
                    {
                        new ContentPartInfo
                        {
                            Type = "input_image",
                            ImageUrl = imageUrl
                        }
                    }
                }
            };
            await session.SendMessageAsync(message, cancellationToken);
        }

        /// <summary>
        ///     テキスト付きで画像を送信します。
        /// </summary>
        /// <param name="session">送信先の <see cref="VoiceLiveSession" /> セッション。</param>
        /// <param name="imageUrl">
        ///     画像の URL またはBase64データURI。
        ///     形式: "data:image/{format};base64,{base64_data}" または HTTPS URL。
        /// </param>
        /// <param name="text">画像に付随するテキスト（例: 質問やプロンプト）。</param>
        /// <param name="cancellationToken">キャンセレーショントークン。</param>
        /// <returns>送信が完了したことを示すタスク。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="imageUrl" /> が null または空の場合。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="text" /> が null または空の場合。</exception>
        public static async Task SendImageWithTextAsync(
            this VoiceLiveSession session,
            string imageUrl,
            string text,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentNullException(nameof(imageUrl), "Image URL or data URI is required.");
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text), "Text is required.");
            }

            var message = new ConversationItemCreateMessage
            {
                Item = new ConversationRequestItem
                {
                    Type = "message",
                    Role = "user",
                    Content = new[]
                    {
                        new ContentPartInfo
                        {
                            Type = "input_text",
                            Text = text
                        },
                        new ContentPartInfo
                        {
                            Type = "input_image",
                            ImageUrl = imageUrl
                        }
                    }
                }
            };
            await session.SendMessageAsync(message, cancellationToken);
        }

        /// <summary>
        ///     マルチモーダルコンテンツを送信します。
        ///     テキスト、画像、音声を任意に組み合わせて送信できます。
        /// </summary>
        /// <param name="session">送信先の <see cref="VoiceLiveSession" /> セッション。</param>
        /// <param name="content">送信するコンテンツパーツの配列。</param>
        /// <param name="cancellationToken">キャンセレーショントークン。</param>
        /// <returns>送信が完了したことを示すタスク。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="content" /> が null または空の場合。</exception>
        public static async Task SendMultiModalContentAsync(
            this VoiceLiveSession session,
            ContentPartInfo[] content,
            CancellationToken cancellationToken = default)
        {
            if (content == null || content.Length == 0)
            {
                throw new ArgumentNullException(nameof(content), "Content parts are required.");
            }

            var message = new ConversationItemCreateMessage
            {
                Item = new ConversationRequestItem
                {
                    Type = "message",
                    Role = "user",
                    Content = content
                }
            };
            await session.SendMessageAsync(message, cancellationToken);
        }

        /// <summary>
        ///     ファイルパスから画像を読み込み、Base64データURIを生成します。
        /// </summary>
        /// <param name="filePath">画像ファイルのパス。</param>
        /// <returns>Base64エンコードされたデータURI（data:image/{format};base64,...）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="filePath" /> が null または空の場合。</exception>
        /// <exception cref="FileNotFoundException">指定されたファイルが存在しない場合。</exception>
        /// <exception cref="NotSupportedException">サポートされていない画像フォーマットの場合。</exception>
        public static string CreateImageDataUri(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath), "File path is required.");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Image file not found.", filePath);
            }

            var extension = Path.GetExtension(filePath);
            if (!SupportedImageFormats.TryGetValue(extension, out var mimeType))
            {
                throw new NotSupportedException(
                    $"Unsupported image format: {extension}. Supported formats: .jpg, .jpeg, .png, .gif, .webp");
            }

            var imageData = File.ReadAllBytes(filePath);
            return CreateImageDataUri(imageData, mimeType);
        }

        /// <summary>
        ///     バイト配列から画像のBase64データURIを生成します。
        /// </summary>
        /// <param name="imageData">画像のバイトデータ。</param>
        /// <param name="mimeType">MIME タイプ（例: "image/png", "image/jpeg"）。</param>
        /// <returns>Base64エンコードされたデータURI（data:{mimeType};base64,...）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="imageData" /> が null または空の場合。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="mimeType" /> が null または空の場合。</exception>
        public static string CreateImageDataUri(byte[] imageData, string mimeType)
        {
            if (imageData == null || imageData.Length == 0)
            {
                throw new ArgumentNullException(nameof(imageData), "Image data is required.");
            }

            if (string.IsNullOrEmpty(mimeType))
            {
                throw new ArgumentNullException(nameof(mimeType), "MIME type is required.");
            }

            var base64 = Convert.ToBase64String(imageData);
            return $"data:{mimeType};base64,{base64}";
        }

        #endregion
    }
}
