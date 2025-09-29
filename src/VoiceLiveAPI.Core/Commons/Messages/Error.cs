// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages
{
    /// <summary>
    ///     Represents an error message in the VoiceLiveAPI.
    /// </summary>
    public class Error : MessageBase
    {
        /// <summary>
        ///     The type of the error message.
        /// </summary>
        public const string TypeName = "error";

        /// <summary>
        ///     Gets or sets the details of the error.
        /// </summary>
        [JsonPropertyName("error")]
        public ErrorDetail ErrorDetail { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Messages.Error" /> class.
        /// </summary>
        public Error()
        {
            Type = TypeName;
            ErrorDetail = new ErrorDetail();
        }
    }
}