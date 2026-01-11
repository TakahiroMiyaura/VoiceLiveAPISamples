// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Represents a session update containing audio transcript delta data.
    /// </summary>
    public class SessionUpdateResponseAudioTranscriptDelta : SessionUpdate
    {
        #region Constants

        /// <summary>
        ///     The type identifier for this session update.
        /// </summary>
        public const string TypeName = "response.audio_transcript.delta";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdateResponseAudioTranscriptDelta" /> class.
        /// </summary>
        /// <param name="message">The underlying message.</param>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        /// <param name="contentIndex">The content index.</param>
        /// <param name="delta">The transcript delta.</param>
        public SessionUpdateResponseAudioTranscriptDelta(
            MessageBase message,
            string responseId,
            string itemId,
            int outputIndex,
            int contentIndex,
            string delta) : base(message)
        {
            ResponseId = responseId ?? string.Empty;
            ItemId = itemId ?? string.Empty;
            OutputIndex = outputIndex;
            ContentIndex = contentIndex;
            Delta = delta ?? string.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the response identifier.
        /// </summary>
        public string ResponseId { get; }

        /// <summary>
        ///     Gets the item identifier.
        /// </summary>
        public string ItemId { get; }

        /// <summary>
        ///     Gets the output index.
        /// </summary>
        public int OutputIndex { get; }

        /// <summary>
        ///     Gets the content index.
        /// </summary>
        public int ContentIndex { get; }

        /// <summary>
        ///     Gets the transcript delta.
        /// </summary>
        public string Delta { get; }

        #endregion
    }
}