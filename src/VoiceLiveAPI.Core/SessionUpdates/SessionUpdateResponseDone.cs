// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Represents a session update indicating that a response is done.
    /// </summary>
    public class SessionUpdateResponseDone : SessionUpdate
    {
        #region Constants

        /// <summary>
        ///     The type identifier for this session update.
        /// </summary>
        public const string TypeName = "response.done";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdateResponseDone" /> class.
        /// </summary>
        /// <param name="message">The underlying message.</param>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="status">The response status.</param>
        public SessionUpdateResponseDone(MessageBase message, string responseId, string status) : base(message)
        {
            ResponseId = responseId ?? string.Empty;
            Status = status ?? string.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the response identifier.
        /// </summary>
        public string ResponseId { get; }

        /// <summary>
        ///     Gets the response status.
        /// </summary>
        public string Status { get; }

        #endregion
    }
}