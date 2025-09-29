// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a message indicating the completion of audio transcription for a conversation item.
    /// </summary>
    public class ConversationItemInputAudioTranscriptionCompleted : MessageBase
    {
        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "conversation.item.input_audio_transcription.completed";

        /// <summary>
        ///     Gets or sets the unique identifier of the conversation item.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the index of the content within the conversation item.
        /// </summary>
        [JsonPropertyName("content_index")]
        public int ContentIndex { get; set; }

        /// <summary>
        ///     Gets or sets the transcript of the audio content.
        /// </summary>
        [JsonPropertyName("transcript")]
        public string Transcript { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConversationItemInputAudioTranscriptionCompleted" /> class.
        /// </summary>
        public ConversationItemInputAudioTranscriptionCompleted()
        {
            EventId = string.Empty;
            Type = TypeName;
            ItemId = string.Empty;
            ContentIndex = 0;
            Transcript = string.Empty;
        }
    }
}