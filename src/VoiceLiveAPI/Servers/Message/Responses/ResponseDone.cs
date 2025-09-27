// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses
{
    /// <summary>
    /// Represents a response message indicating that a process has completed.
    /// This message is sent by the server when a response has finished processing.
    /// </summary>
    public class ResponseDone : MessageBase
    {
        #region Static Fields

        /// <summary>
        /// The type identifier for the response.done message.
        /// </summary>
        public const string Type = "response.done";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the response details associated with the completion.
        /// Contains the final state of the completed response.
        /// </summary>
        /// <value>
        /// A <see cref="Response"/> object representing the completed response.
        /// </value>
        public Response response { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseDone"/> class.
        /// Sets the message type and initializes an empty response object.
        /// </summary>
        public ResponseDone()
        {
            type = Type;
            response = new Response();
        }

        #endregion
    }
}