// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a message indicating that a response has been created.
    /// </summary>
    public class ResponseCreated : MessageBase
    {
        /// <summary>
        ///     The type identifier for the "response.created" message.
        /// </summary>
        public const string TypeName = "response.created";

        /// <summary>
        ///     Gets or sets the response associated with this message.
        /// </summary>
        [JsonPropertyName("response")]
        public Response Response { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseCreated" /> class.
        /// </summary>
        public ResponseCreated()
        {
            Type = TypeName;
            Response = new Response();
        }
    }
}