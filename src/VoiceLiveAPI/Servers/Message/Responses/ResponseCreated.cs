// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Servers.Message.Responses
{
    /// <summary>
    /// Represents a message indicating that a response has been created.
    /// This message is sent by the server when a new response begins processing.
    /// </summary>
    public class ResponseCreated : MessageBase
    {
        #region Static Fields

        /// <summary>
        /// The type identifier for the "response.created" message.
        /// </summary>
        public const string Type = "response.created";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the response associated with this message.
        /// Contains the initial state of the newly created response.
        /// </summary>
        /// <value>
        /// A <see cref="Response"/> object representing the created response.
        /// </value>
        public Response response { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseCreated"/> class.
        /// Sets the message type and initializes an empty response object.
        /// </summary>
        public ResponseCreated()
        {
            type = Type;
            response = new Response();
        }

        #endregion
    }
}