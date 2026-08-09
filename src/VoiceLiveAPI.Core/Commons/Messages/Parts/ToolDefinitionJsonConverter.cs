// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     JSON converter for polymorphic <see cref="ToolDefinition" /> deserialization.
    /// </summary>
    /// <remarks>
    ///     Routes deserialization to the correct derived type based on the "type" discriminator field.
    /// </remarks>
    public class ToolDefinitionJsonConverter : JsonConverter<ToolDefinition>
    {
        #region Public Methods

        /// <summary>
        ///     Reads and converts JSON to the appropriate <see cref="ToolDefinition" /> subclass.
        /// </summary>
        /// <param name="reader">The reader to read JSON from.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The deserialized <see cref="ToolDefinition" /> instance.</returns>
        public override ToolDefinition Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            var type = root.TryGetProperty("type", out var typeProp)
                ? typeProp.GetString()
                : null;

            switch (type)
            {
                case "mcp":
                    return root.Deserialize<McpToolConfig>(ConverterOptions)
                           ?? throw new JsonException("Failed to deserialize McpToolConfig.");
                case "foundry_agent":
                    return root.Deserialize<FoundryAgentToolConfig>(ConverterOptions)
                           ?? throw new JsonException("Failed to deserialize FoundryAgentToolConfig.");
                case "function":
                default:
                    return root.Deserialize<Function>(ConverterOptions)
                           ?? throw new JsonException("Failed to deserialize Function.");
            }
        }

        /// <summary>
        ///     Writes a <see cref="ToolDefinition" /> as JSON.
        /// </summary>
        /// <param name="writer">The writer to write JSON to.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, ToolDefinition value,
            JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), ConverterOptions);
        }

        #endregion

        #region Private Fields

        /// <summary>
        ///     Options without this converter to prevent infinite recursion.
        /// </summary>
        private static readonly JsonSerializerOptions ConverterOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        #endregion
    }
}
