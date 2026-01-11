// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents session information returned by the VoiceLive service.
    /// </summary>
    /// <remarks>
    ///     This class provides a unified representation of session data
    ///     for both session creation and update events.
    /// </remarks>
    public class SessionInfo : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for session created events.
        /// </summary>
        public const string TypeNameCreated = "session.created";

        /// <summary>
        ///     The type identifier for session updated events.
        /// </summary>
        public const string TypeNameUpdated = "session.updated";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the event type (session.created or session.updated).
        /// </summary>
        /// <remarks>
        ///     This property is set based on the incoming event type.
        /// </remarks>
        private readonly string type;

        /// <inheritdoc />
        public override string Type => type;

        /// <summary>
        ///     Gets or sets the session identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the object type (typically "realtime.session").
        /// </summary>
        [JsonPropertyName("object")]
        public string Object { get; set; }

        /// <summary>
        ///     Gets or sets the model being used for this session.
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        ///     Gets or sets the expiration timestamp for this session.
        /// </summary>
        [JsonPropertyName("expires_at")]
        public long? ExpiresAt { get; set; }

        /// <summary>
        ///     Gets or sets the voice configuration for this session.
        /// </summary>
        [JsonPropertyName("voice")]
        public string Voice { get; set; }

        /// <summary>
        ///     Gets or sets the system instructions for this session.
        /// </summary>
        [JsonPropertyName("instructions")]
        public string Instructions { get; set; }

        /// <summary>
        ///     Gets or sets the modalities enabled for this session.
        /// </summary>
        [JsonPropertyName("modalities")]
        public string[] Modalities { get; set; }

        /// <summary>
        ///     Gets or sets the temperature for response generation.
        /// </summary>
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        /// <summary>
        ///     Gets or sets the maximum number of output tokens.
        /// </summary>
        [JsonPropertyName("max_output_tokens")]
        public object MaxOutputTokens { get; set; }

        /// <summary>
        ///     Gets or sets the avatar configuration for this session.
        /// </summary>
        [JsonPropertyName("avatar")]
        public Avatar Avatar { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionInfo" /> class.
        /// </summary>
        public SessionInfo()
        {
            type = "session.created";
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionInfo" /> class with the specified type.
        /// </summary>
        /// <param name="type">The event type (session.created or session.updated).</param>
        public SessionInfo(string type)
        {
            this.type = type ?? "session.created";
        }

        #endregion
    }
}