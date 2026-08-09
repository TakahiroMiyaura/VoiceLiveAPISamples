// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a function call arguments delta in a response.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ResponseFunctionCallArgumentsDeltaMessage</c> class.
    /// </remarks>
    public class FunctionCallDelta : ToolCallArgumentsDelta
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type name for this event.
        /// </summary>
        public const string TypeName = "response.function_call_arguments.delta";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the call identifier.
        /// </summary>
        [JsonPropertyName("call_id")]
        public string CallId { get; set; }

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
