// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents the completion of a function call in a response.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ResponseFunctionCallArgumentsDoneMessage</c> class.
    /// </remarks>
    public class FunctionCallDone : ServerEvent
    {
        #region Properties

        /// <inheritdoc />
        public override string Type => "response.function_call_arguments.done";

        /// <summary>
        ///     Gets or sets the response identifier.
        /// </summary>
        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; }

        /// <summary>
        ///     Gets or sets the item identifier.
        /// </summary>
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        /// <summary>
        ///     Gets or sets the output index.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the call identifier.
        /// </summary>
        [JsonPropertyName("call_id")]
        public string CallId { get; set; }

        /// <summary>
        ///     Gets or sets the complete function arguments.
        /// </summary>
        [JsonPropertyName("arguments")]
        public string Arguments { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionCallDone" /> class.
        /// </summary>
        public FunctionCallDone()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionCallDone" /> class with specified values.
        /// </summary>
        public FunctionCallDone(string eventId, string responseId, string itemId, int outputIndex, string callId,
            string arguments)
        {
            EventId = eventId;
            ResponseId = responseId;
            ItemId = itemId;
            OutputIndex = outputIndex;
            CallId = callId;
            Arguments = arguments;
        }

        #endregion
    }
}