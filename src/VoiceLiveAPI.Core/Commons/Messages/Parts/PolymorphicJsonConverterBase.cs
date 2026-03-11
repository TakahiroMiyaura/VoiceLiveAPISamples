// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts
{
    /// <summary>
    ///     Base class for polymorphic JSON converters that discriminate types using the "type" property.
    /// </summary>
    /// <typeparam name="TBase">The abstract base type to convert.</typeparam>
    /// <remarks>
    ///     Provides shared infrastructure for type-discriminated JSON deserialization,
    ///     including <see cref="CreateOptionsWithoutConverter" /> to prevent infinite recursion.
    ///     Compatible with .NET Standard 2.0.
    /// </remarks>
    public abstract class PolymorphicJsonConverterBase<TBase> : JsonConverter<TBase>
    {
        #region Public Methods

        /// <summary>
        ///     Reads and converts the JSON to a <typeparamref name="TBase" /> instance.
        /// </summary>
        /// <param name="reader">The reader to read JSON from.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>A <typeparamref name="TBase" /> instance.</returns>
        /// <exception cref="JsonException">Thrown when the type discriminator is missing or unknown.</exception>
        public override TBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            if (!root.TryGetProperty("type", out var typeProp))
            {
                throw new JsonException($"Missing 'type' property for {typeof(TBase).Name}.");
            }

            var typeValue = typeProp.GetString();
            var rawText = root.GetRawText();
            var optionsWithoutConverter = CreateOptionsWithoutConverter(options);

            return DeserializeByType(typeValue, rawText, optionsWithoutConverter);
        }

        /// <summary>
        ///     Writes a <typeparamref name="TBase" /> instance as JSON.
        /// </summary>
        /// <param name="writer">The writer to write JSON to.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options)
        {
            var optionsWithoutConverter = CreateOptionsWithoutConverter(options);
            SerializeByType(writer, value, optionsWithoutConverter);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Deserializes the JSON to a concrete type based on the type discriminator value.
        /// </summary>
        /// <param name="typeValue">The value of the "type" discriminator property.</param>
        /// <param name="rawText">The raw JSON text to deserialize.</param>
        /// <param name="options">The serializer options without this converter.</param>
        /// <returns>A <typeparamref name="TBase" /> instance of the appropriate concrete type.</returns>
        /// <exception cref="JsonException">Thrown when the type discriminator is unknown.</exception>
        protected abstract TBase DeserializeByType(string typeValue, string rawText, JsonSerializerOptions options);

        /// <summary>
        ///     Serializes a <typeparamref name="TBase" /> instance to JSON based on its concrete type.
        /// </summary>
        /// <param name="writer">The writer to write JSON to.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="options">The serializer options without this converter.</param>
        protected abstract void SerializeByType(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options);

        #endregion

        #region Private Methods

        /// <summary>
        ///     Creates a copy of the serializer options without this converter to prevent infinite recursion.
        /// </summary>
        /// <param name="options">The original serializer options.</param>
        /// <returns>A new <see cref="JsonSerializerOptions" /> instance without this converter.</returns>
        private JsonSerializerOptions CreateOptionsWithoutConverter(JsonSerializerOptions options)
        {
            var converterType = GetType();
            var newOptions = new JsonSerializerOptions();

            foreach (var converter in options.Converters)
            {
                if (converter.GetType() != converterType)
                {
                    newOptions.Converters.Add(converter);
                }
            }

            newOptions.PropertyNamingPolicy = options.PropertyNamingPolicy;
            newOptions.DefaultIgnoreCondition = options.DefaultIgnoreCondition;
            newOptions.WriteIndented = options.WriteIndented;

            return newOptions;
        }

        #endregion
    }
}
