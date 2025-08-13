// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Commons.Messages;

/// <summary>
/// Represents the base information of a conversation item.
/// </summary>
public class ConversationItemBaseInfo
{
    /// <summary>
    /// The unique ID of the Item. The client can specify the ID to help manage server-side context. 
    /// If the client doesn't provide an ID, the server generates one.
    /// </summary>
    public string id { get; set; } = string.Empty;

    /// <summary>
    /// The type of the conversation item. Possible values:
    /// - message
    /// - function_call
    /// - function_call_output
    /// </summary>
    public string type { get; set; } = string.Empty;

    /// <summary>
    /// The object information of the conversation item.
    /// </summary>
    [JsonPropertyName("object")]
    public string objectInfo { get; set; } = "realtime.Item";

    /// <summary>
    /// The status of the conversation item. Possible values:
    /// - in_progress
    /// - completed
    /// - incomplete
    /// </summary>
    public string? status { get; set; }

    /// <summary>
    /// The role of the conversation item. Possible values:
    /// - system
    /// - user
    /// - assistant
    /// </summary>
    public string role { get; set; } = "user";

    /// <summary>
    /// The content parts of the conversation item.
    /// </summary>
    public ContentPartInfo[]? content { get; set; }

    /// <summary>
    /// The call ID associated with the conversation item.
    /// </summary>
    public string? call_id { get; set; }

    /// <summary>
    /// The name of the conversation item.
    /// </summary>
    public string? name { get; set; }

    /// <summary>
    /// The arguments of the conversation item.
    /// </summary>
    public string? arguments { get; set; }

    /// <summary>
    /// The output of the conversation item.
    /// </summary>
    public string? output { get; set; }
}
