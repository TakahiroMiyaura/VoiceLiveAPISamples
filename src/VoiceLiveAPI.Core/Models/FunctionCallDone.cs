// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents the completion of a function call in a response.
    /// </summary>
    /// <remarks>
    ///     This is the recommended replacement for the legacy <c>ResponseFunctionCallArgumentsDoneMessage</c> class.
    /// </remarks>
    public class FunctionCallDone : ToolCallArgumentsDone
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type name for this event.
        /// </summary>
        public const string TypeName = "response.function_call_arguments.done";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the call identifier.
        /// </summary>
        [JsonPropertyName("call_id")]
        public string CallId { get; set; }

        /// <summary>
        ///     Gets or sets the name of the function being called.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

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
        /// <param name="eventId">The event identifier.</param>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        /// <param name="callId">The call identifier.</param>
        /// <param name="name">The function name.</param>
        /// <param name="arguments">The function arguments as a JSON string.</param>
        public FunctionCallDone(string eventId, string responseId, string itemId, int outputIndex, string callId,
            string name, string arguments)
        {
            EventId = eventId;
            ResponseId = responseId;
            ItemId = itemId;
            OutputIndex = outputIndex;
            CallId = callId;
            Name = name;
            Arguments = arguments;
        }

        #endregion
    }
}
