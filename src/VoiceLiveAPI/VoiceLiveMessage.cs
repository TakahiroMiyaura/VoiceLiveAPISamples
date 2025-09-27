// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{
    /// <summary>
    /// Represents a base message from the VoiceLive API.
    /// This is the root class for all message types exchanged with the VoiceLive service.
    /// </summary>
    public class VoiceLiveMessage
    {
        #region Properties

        /// <summary>
        /// Gets or sets the message type identifier.
        /// This field specifies the type of message being sent or received.
        /// </summary>
        /// <value>
        /// A string representing the message type. Defaults to an empty string.
        /// </value>
        public string type { get; set; } = string.Empty;

        #endregion
    }
}