// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Represents a video delta containing a streaming avatar video frame from the AI response.
    /// </summary>
    /// <remarks>
    ///     Delivered as <c>response.video.delta</c> when the avatar is configured with
    ///     <c>output_protocol=websocket</c> (available in API version 2026-06-01-preview and later).
    ///     The frame payload is base64-encoded and uses the codec named by <see cref="Codec" /> (e.g. h264).
    /// </remarks>
    public class VideoDelta : ServerEvent
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this event.
        /// </summary>
        public const string TypeName = "response.video.delta";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the output index in the response.
        /// </summary>
        [JsonPropertyName("output_index")]
        public int OutputIndex { get; set; }

        /// <summary>
        ///     Gets or sets the codec used for the video data (for example, <c>h264</c>).
        /// </summary>
        [JsonPropertyName("codec")]
        public string Codec { get; set; }

        /// <summary>
        ///     Gets or sets the base64-encoded video frame data.
        /// </summary>
        [JsonPropertyName("delta")]
        public string Delta { get; set; }

        /// <summary>
        ///     Gets the decoded video frame as a byte array.
        /// </summary>
        /// <value>
        ///     The decoded video bytes, or an empty array if <see cref="Delta" /> is null or empty.
        /// </value>
        public ReadOnlyMemory<byte> VideoData
        {
            get
            {
                if (string.IsNullOrEmpty(Delta))
                {
                    return ReadOnlyMemory<byte>.Empty;
                }

                return Convert.FromBase64String(Delta);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="VideoDelta" /> class.
        /// </summary>
        public VideoDelta()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VideoDelta" /> class with the specified values.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        /// <param name="codec">The codec used for the video data.</param>
        /// <param name="delta">The base64-encoded video frame data.</param>
        public VideoDelta(string eventId, int outputIndex, string codec, string delta)
        {
            EventId = eventId;
            OutputIndex = outputIndex;
            Codec = codec;
            Delta = delta;
        }

        #endregion
    }
}
