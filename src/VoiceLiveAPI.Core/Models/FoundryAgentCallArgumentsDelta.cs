// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a Foundry Agent call arguments delta during streaming.
    /// </summary>
    /// <remarks>
    ///     This event is fired while arguments for a Foundry Agent call are being streamed.
    ///     Available in API version 2026-01-01-preview and later.
    /// </remarks>
    public class FoundryAgentCallArgumentsDelta : FoundryAgentCallArgumentsBase
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type name for this event.
        /// </summary>
        public const string TypeName = "response.foundry_agent_call_arguments.delta";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the arguments delta.
        /// </summary>
        [JsonPropertyName("delta")]
        public string Delta { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FoundryAgentCallArgumentsDelta" /> class.
        /// </summary>
        public FoundryAgentCallArgumentsDelta()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FoundryAgentCallArgumentsDelta" /> class with specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        /// <param name="delta">The arguments delta.</param>
        public FoundryAgentCallArgumentsDelta(string eventId, string responseId, string itemId, int outputIndex,
            string delta) : base(eventId, responseId, itemId, outputIndex)
        {
            Delta = delta;
        }

        #endregion
    }
}
