// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified
{
    /// <summary>
    ///     Represents the base information of a conversation item.
    /// </summary>
    public class ConversationItemBaseInfo
    {
        /// <summary>
        ///     The unique ID of the Item. The client can specify the ID to help manage server-side context.
        ///     If the client doesn't provide an ID, the server generates one.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        ///     The type of the conversation item. Possible values:
        ///     - message
        ///     - function_call
        ///     - function_call_output
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        ///     The object information of the conversation item.
        /// </summary>
        [JsonPropertyName("object")]
        public string ObjectInfo { get; set; } = "realtime.Item";

        /// <summary>
        ///     The status of the conversation item. Possible values:
        ///     - in_progress
        ///     - completed
        ///     - incomplete
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = null;

        /// <summary>
        ///     The role of the conversation item. Possible values:
        ///     - system
        ///     - user
        ///     - assistant
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        /// <summary>
        ///     The content parts of the conversation item.
        /// </summary>
        [JsonPropertyName("content")]
        public ContentPartInfo[] Content { get; set; } = null;

        /// <summary>
        ///     The call ID associated with the conversation item.
        /// </summary>
        [JsonPropertyName("call_id")]
        public string CallId { get; set; } = null;

        /// <summary>
        ///     The name of the conversation item.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null;

        /// <summary>
        ///     The arguments of the conversation item.
        /// </summary>
        [JsonPropertyName("arguments")]
        public string Arguments { get; set; } = null;

        /// <summary>
        ///     The output of the conversation item.
        /// </summary>
        [JsonPropertyName("output")]
        public string Output { get; set; } = null;
    }
}