// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models.Commands
{
    /// <summary>
    ///     Represents a command to create a new response.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ResponseCreateMessage</c> class.
    /// </remarks>
    public class ResponseCreate
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The message type for this command.
        /// </summary>
        public const string TypeName = "response.create";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the event identifier.
        /// </summary>
        public string EventId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseCreate" /> class.
        /// </summary>
        public ResponseCreate()
        {
        }

        #endregion
    }
}