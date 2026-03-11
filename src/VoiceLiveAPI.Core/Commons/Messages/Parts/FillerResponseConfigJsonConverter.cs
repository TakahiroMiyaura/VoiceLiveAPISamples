// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Custom JSON converter for <see cref="FillerResponseConfig" /> polymorphic deserialization.
    /// </summary>
    /// <remarks>
    ///     Discriminates between <see cref="BasicFillerResponseConfig" /> (type: "static_filler") and
    ///     <see cref="LlmFillerResponseConfig" /> (type: "llm_filler") based on the "type" JSON property.
    ///     Compatible with .NET Standard 2.0.
    /// </remarks>
    public class FillerResponseConfigJsonConverter : PolymorphicJsonConverterBase<FillerResponseConfig>
    {
        #region Protected Methods

        /// <inheritdoc />
        protected override FillerResponseConfig DeserializeByType(string typeValue, string rawText,
            JsonSerializerOptions options)
        {
            switch (typeValue)
            {
                case BasicFillerResponseConfig.TypeDiscriminator:
                    return JsonSerializer.Deserialize<BasicFillerResponseConfig>(rawText, options);
                case LlmFillerResponseConfig.TypeDiscriminator:
                    return JsonSerializer.Deserialize<LlmFillerResponseConfig>(rawText, options);
                default:
                    throw new JsonException($"Unknown FillerResponseConfig type: '{typeValue}'.");
            }
        }

        /// <inheritdoc />
        protected override void SerializeByType(Utf8JsonWriter writer, FillerResponseConfig value,
            JsonSerializerOptions options)
        {
            switch (value)
            {
                case BasicFillerResponseConfig basic:
                    JsonSerializer.Serialize(writer, basic, options);
                    break;
                case LlmFillerResponseConfig llm:
                    JsonSerializer.Serialize(writer, llm, options);
                    break;
                default:
                    JsonSerializer.Serialize(writer, value, value.GetType(), options);
                    break;
            }
        }

        #endregion
    }
}
