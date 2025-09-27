// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Sessions
{

    /// <summary>  
    /// Represents a message indicating that a client session has been updated.  
    /// </summary>  
    public class ClientSessionUpdate : MessageBase
    {
        /// <summary>  
        /// The type of the message, indicating a session update.  
        /// </summary>  
        public const string Type = "session.update";

        /// <summary>  
        /// Gets or sets the updated client session details.  
        /// </summary>  
        public ClientSession session { get; set; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSessionUpdate"/> class.
        /// </summary>
        public ClientSessionUpdate()
        {
            type = Type;
        }

        /// <summary>
        /// クライアントセッション更新メッセージのデフォルト値を設定します。
        /// </summary>
        /// <returns>デフォルト値が設定されたクライアントセッション更新メッセージ。</returns>
        public static ClientSessionUpdate Default => new ClientSessionUpdate()
        {
            session = new ClientSession()
            {
                modalities = new[] { "text","audio" },
                turn_detection = new TurnDetection()
                {
                    type = "server_vad",
                    end_of_utterance_detection = new
                    {
                        model = "semantic_detection_v1",
                        threshold = 0.1,
                        timeout = 4
                    }
                },
                input_audio_sampling_rate = 24000,
                input_audio_noise_reduction = new AudioInputAudioNoiseReductionSettings()
                    { type = "azure_deep_noise_suppression" },
                voice = new Voice()
                {
                    name = "en-US-AvaNeural",
                    type = "azure-standard"
                },
                output_audio_timestamp_types = new[] { "word" },
                animation = new Animation()
                {
                    outputs = new []{ "viseme_id" }
                },
                //avatar = new Avatar()
                //{
                //    character = "lisa",
                //    style = "casual-sitting",
                //    customized = false,
                //    video = new Video()
                //    {
                //        bitrate = 2000000,
                //        codec = "h264",
                //        crop = new Crop()
                //        {
                //            top_left = new []{560,0},
                //            bottom_right = new[] { 1360, 1080 }
                //        },
                //        resolution = new Resolution()
                //        {
                //            width = 1920,
                //            height = 1080
                //        },
                //        background = new Background()
                //        {
                //            color = "#00FF00FF"
                //        }
                //    }
                //}
            }
        };
    }
}
