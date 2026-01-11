// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a function call arguments delta in a response.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ResponseFunctionCallArgumentsDeltaMessage</c> class.
    /// </remarks>
    public class FunctionCallDelta : ServerEvent
    {
        #region Properties

        /// <inheritdoc />
        public override string Type => "response.function_call_arguments.delta";

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
        ///     Gets or sets the function arguments delta.
        /// </summary>
        [JsonPropertyName("delta")]
        public string Delta { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionCallDelta" /> class.
        /// </summary>
        public FunctionCallDelta()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionCallDelta" /> class with specified values.
        /// </summary>
        public FunctionCallDelta(string eventId, string responseId, string itemId, int outputIndex, string callId,
            string delta)
        {
            EventId = eventId;
            ResponseId = responseId;
            ItemId = itemId;
            OutputIndex = outputIndex;
            CallId = callId;
            Delta = delta;
        }

        #endregion
    }
}