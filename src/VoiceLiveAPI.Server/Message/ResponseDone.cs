// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Message
{
    /// <summary>
    ///     Represents a response message indicating that a process has completed.
    /// </summary>
    public class ResponseDone : MessageBase
    {
        /// <summary>
        ///     The type identifier for the response.done message.
        /// </summary>
        public const string TypeName = "response.done";

        /// <summary>
        ///     Gets or sets the response details associated with the completion.
        /// </summary>
        [JsonPropertyName("response")]
        public Response Response { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseDone" /> class.
        /// </summary>
        public ResponseDone()
        {
            Type = TypeName;
            Response = new Response();
        }
    }
}