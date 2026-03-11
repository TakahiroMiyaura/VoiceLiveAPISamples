// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents the completion of Foundry Agent call arguments streaming.
    /// </summary>
    /// <remarks>
    ///     This event is fired when all arguments for a Foundry Agent call have been received.
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    public class FoundryAgentCallArgumentsDone : FoundryAgentCallArgumentsBase
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type name for this event.
        /// </summary>
        public const string TypeName = "response.foundry_agent_call_arguments.done";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the complete arguments as a JSON string.
        /// </summary>
        [JsonPropertyName("arguments")]
        public string Arguments { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FoundryAgentCallArgumentsDone" /> class.
        /// </summary>
        public FoundryAgentCallArgumentsDone()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FoundryAgentCallArgumentsDone" /> class with specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        /// <param name="arguments">The complete arguments JSON string.</param>
        public FoundryAgentCallArgumentsDone(string eventId, string responseId, string itemId, int outputIndex,
            string arguments) : base(eventId, responseId, itemId, outputIndex)
        {
            Arguments = arguments;
        }

        #endregion
    }
}
