// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json.Serialization;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts.Unverified;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages
{
    /// <summary>
    ///     Represents a conversation item create message.
    /// </summary>
    /// <remarks>
    ///     This message is used to add new items to the conversation context,
    ///     including messages, function calls, and function call outputs.
    /// </remarks>
    public class ConversationItemCreateMessage : ClientCommand
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The type identifier for this message.
        /// </summary>
        public const string TypeName = "conversation.item.create";

        #endregion

        #region Properties

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        ///     Gets or sets the ID of the preceding item after which the new item will be inserted.
        ///     If null, the item is appended to the end of the conversation.
        /// </summary>
        [JsonPropertyName("previous_item_id")]
        public string PreviousItemId { get; set; } = null;

        /// <summary>
        ///     Gets or sets the item to create.
        /// </summary>
        [JsonPropertyName("item")]
        public ConversationRequestItem Item { get; set; } = null;

        #endregion
    }

    /// <summary>
    ///     Represents a request item in a conversation.
    /// </summary>
    /// <remarks>
    ///     Supported item types:
    ///     <list type="bullet">
    ///         <item>
    ///             <term>message</term>
    ///             <description>A message from user, assistant, or system.</description>
    ///         </item>
    ///         <item>
    ///             <term>function_call</term>
    ///             <description>A function call made by the assistant.</description>
    ///         </item>
    ///         <item>
    ///             <term>function_call_output</term>
    ///             <description>The output/result of a function call.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public class ConversationRequestItem
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the type of the item.
        ///     Valid values: "message", "function_call", "function_call_output".
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null;

        /// <summary>
        ///     Gets or sets the unique ID of the item.
        ///     The client can specify the ID to help manage server-side context.
        ///     If not provided, the server generates one.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null;

        /// <summary>
        ///     Gets or sets the role of the message sender.
        ///     Valid values: "user", "assistant", "system".
        ///     Used when Type is "message".
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = null;

        /// <summary>
        ///     Gets or sets the content parts of the message.
        ///     Used when Type is "message".
        /// </summary>
        [JsonPropertyName("content")]
        public ContentPartInfo[] Content { get; set; } = null;

        /// <summary>
        ///     Gets or sets the ID of the function call this output is responding to.
        ///     Used when Type is "function_call_output".
        /// </summary>
        [JsonPropertyName("call_id")]
        public string CallId { get; set; } = null;

        /// <summary>
        ///     Gets or sets the output of the function call as a JSON string.
        ///     Used when Type is "function_call_output".
        /// </summary>
        [JsonPropertyName("output")]
        public string Output { get; set; } = null;

        /// <summary>
        ///     Gets or sets the name of the function being called.
        ///     Used when Type is "function_call".
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null;

        /// <summary>
        ///     Gets or sets the arguments for the function call as a JSON string.
        ///     Used when Type is "function_call".
        /// </summary>
        [JsonPropertyName("arguments")]
        public string Arguments { get; set; } = null;

        #endregion
    }
}
