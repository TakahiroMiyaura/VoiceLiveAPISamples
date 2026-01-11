// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Represents a session update indicating that input audio transcription has completed.
    /// </summary>
    public class SessionUpdateTranscriptionCompleted : SessionUpdate
    {
        #region Constants

        /// <summary>
        ///     The type identifier for this session update.
        /// </summary>
        public const string TypeName = "conversation.item.input_audio_transcription.completed";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdateTranscriptionCompleted" /> class.
        /// </summary>
        /// <param name="message">The underlying message.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="contentIndex">The content index.</param>
        /// <param name="transcript">The transcript text.</param>
        public SessionUpdateTranscriptionCompleted(MessageBase message, string itemId, int contentIndex,
            string transcript) : base(message)
        {
            ItemId = itemId ?? string.Empty;
            ContentIndex = contentIndex;
            Transcript = transcript ?? string.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the item identifier.
        /// </summary>
        public string ItemId { get; }

        /// <summary>
        ///     Gets the content index.
        /// </summary>
        public int ContentIndex { get; }

        /// <summary>
        ///     Gets the transcript text.
        /// </summary>
        public string Transcript { get; }

        #endregion
    }
}