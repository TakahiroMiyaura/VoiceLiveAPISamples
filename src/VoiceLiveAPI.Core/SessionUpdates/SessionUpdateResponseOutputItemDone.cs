// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Represents a session update indicating that a response output item is done.
    /// </summary>
    public class SessionUpdateResponseOutputItemDone : SessionUpdate
    {
        #region Constants

        /// <summary>
        ///     The type identifier for this session update.
        /// </summary>
        public const string TypeName = "response.output_item.done";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdateResponseOutputItemDone" /> class.
        /// </summary>
        /// <param name="message">The underlying message.</param>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        /// <param name="item">The output item.</param>
        public SessionUpdateResponseOutputItemDone(MessageBase message, string responseId, int outputIndex, Item item)
            : base(message)
        {
            ResponseId = responseId ?? string.Empty;
            OutputIndex = outputIndex;
            Item = item;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the response identifier.
        /// </summary>
        public string ResponseId { get; }

        /// <summary>
        ///     Gets the output index.
        /// </summary>
        public int OutputIndex { get; }

        /// <summary>
        ///     Gets the output item.
        /// </summary>
        public Item Item { get; }

        #endregion
    }
}