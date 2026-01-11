// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates
{
    /// <summary>
    ///     Represents an unknown or unhandled session update type.
    /// </summary>
    public class SessionUpdateUnknown : SessionUpdate
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionUpdateUnknown" /> class.
        /// </summary>
        /// <param name="message">The underlying message.</param>
        public SessionUpdateUnknown(MessageBase message) : base(message)
        {
        }

        #endregion
    }
}