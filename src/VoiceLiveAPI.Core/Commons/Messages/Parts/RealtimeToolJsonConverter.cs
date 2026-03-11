// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Custom JSON converter for <see cref="RealtimeTool" /> polymorphic deserialization.
    /// </summary>
    /// <remarks>
    ///     Discriminates between <see cref="Function" /> (type: "function") and
    ///     <see cref="FoundryAgentTool" /> (type: "foundry_agent") based on the "type" JSON property.
    ///     Compatible with .NET Standard 2.0.
    /// </remarks>
    public class RealtimeToolJsonConverter : PolymorphicJsonConverterBase<RealtimeTool>
    {
        #region Protected Methods

        /// <inheritdoc />
        protected override RealtimeTool DeserializeByType(string typeValue, string rawText,
            JsonSerializerOptions options)
        {
            switch (typeValue)
            {
                case Function.TypeDiscriminator:
                    return JsonSerializer.Deserialize<Function>(rawText, options);
                case FoundryAgentTool.TypeDiscriminator:
                    return JsonSerializer.Deserialize<FoundryAgentTool>(rawText, options);
                default:
                    throw new JsonException($"Unknown RealtimeTool type: '{typeValue}'.");
            }
        }

        /// <inheritdoc />
        protected override void SerializeByType(Utf8JsonWriter writer, RealtimeTool value,
            JsonSerializerOptions options)
        {
            switch (value)
            {
                case Function function:
                    JsonSerializer.Serialize(writer, function, options);
                    break;
                case FoundryAgentTool foundryAgent:
                    JsonSerializer.Serialize(writer, foundryAgent, options);
                    break;
                default:
                    JsonSerializer.Serialize(writer, value, value.GetType(), options);
                    break;
            }
        }

        #endregion
    }
}
