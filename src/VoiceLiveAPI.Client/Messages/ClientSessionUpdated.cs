// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Client.Messages
{
    /// <summary>
    ///     Represents a message indicating that a client session has been updated.
    /// </summary>
    public class ClientSessionUpdate : MessageBase, IClientSessionUpdate
    {
        /// <summary>
        ///     The type of the message, indicating a session update.
        /// </summary>
        public const string TypeName = "session.update";

        /// <summary>
        ///     Gets or sets the updated client session details.
        /// </summary>
        [JsonPropertyName("session")]
        public ClientSession Session { get; set; }

        /// <summary>
        ///     クライアントセッション更新メッセージのデフォルト値を設定します。
        /// </summary>
        /// <returns>デフォルト値が設定されたクライアントセッション更新メッセージ。</returns>
        public static ClientSessionUpdate Default => new ClientSessionUpdate()
        {
            Session = new ClientSession
            {
                Modalities = new[] { "text", "audio" },
                TurnDetection = new TurnDetection
                {
                    Type = "server_vad",
                    EndOfUtteranceDetection = new
                    {
                        model = "semantic_detection_v1",
                        threshold = 0.1,
                        timeout = 4
                    }
                },
                InputAudioSamplingRate = 24000,
                InputAudioNoiseReduction = new AudioInputAudioNoiseReductionSettings
                    { Type = "azure_deep_noise_suppression" },
                Voice = new Voice
                {
                    Name = "ja-JP-Nanami:DragonHDLatestNeural",
                    Type = "azure-standard"
                },
                OutputAudioTimestampTypes = new[] { "word" },
                Animation = new Animation
                {
                    Outputs = new[] { "viseme_id" }
                },
                Avatar = new Avatar
                {
                    Character = "lisa",
                    Style = "casual-sitting",
                    Customized = false,
                    Video = new Video
                    {
                        BitRate = 2000000,
                        Codec = "h264",
                        Crop = new Crop
                        {
                            TopLeft = new[] { 560, 0 },
                            BottomRight = new[] { 1360, 1080 }
                        },
                        Resolution = new Resolution
                        {
                            Width = 1920,
                            Height = 1080
                        },
                        Background = new Background
                        {
                            Color = "#FFFFFFFF"
                        }
                    }
                }
            }
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientSessionUpdate" /> class.
        /// </summary>
        public ClientSessionUpdate()
        {
            Type = TypeName;
        }
    }
}