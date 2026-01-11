// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text.Json;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models
{
    /// <summary>
    ///     Factory for creating VoiceLive model instances.
    /// </summary>
    /// <remarks>
    ///     This factory follows the Azure SDK pattern for creating model instances.
    ///     It provides static methods for creating model objects, which is useful for testing
    ///     and for creating models from raw JSON data.
    /// </remarks>
    public static class VoiceLiveModelFactory
    {
        #region SessionInfo Factory Methods

        /// <summary>
        ///     Creates a <see cref="SessionInfo" /> from a JSON element.
        /// </summary>
        /// <param name="element">The JSON element containing the session data.</param>
        /// <returns>A new <see cref="SessionInfo" /> instance.</returns>
        public static SessionInfo SessionInfoFromJson(JsonElement element)
        {
            var eventType = GetStringProperty(element, "type");
            var sessionElement = element.TryGetProperty("session", out var session) ? session : element;
            var info = new SessionInfo(eventType)
            {
                EventId = GetStringProperty(element, "event_id"),
                Id = GetStringProperty(sessionElement, "id"),
                Object = GetStringProperty(sessionElement, "object"),
                Model = GetStringProperty(sessionElement, "model"),
                Voice = GetStringProperty(sessionElement, "voice"),
                Instructions = GetStringProperty(sessionElement, "instructions")
            };

            if (sessionElement.TryGetProperty("expires_at", out var expiresAt) &&
                expiresAt.TryGetInt64(out var expiresAtValue))
            {
                info.ExpiresAt = expiresAtValue;
            }

            if (sessionElement.TryGetProperty("temperature", out var temp) &&
                temp.TryGetDouble(out var tempValue))
            {
                info.Temperature = tempValue;
            }

            if (sessionElement.TryGetProperty("avatar", out var avatar))
            {
                info.Avatar = avatar.Deserialize<Avatar>();
            }

            return info;
        }

        #endregion

        #region AudioDelta Factory Methods

        /// <summary>
        ///     Creates a new <see cref="AudioDelta" /> instance.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        /// <param name="contentIndex">The content index.</param>
        /// <param name="delta">The base64-encoded audio data.</param>
        /// <returns>A new <see cref="AudioDelta" /> instance.</returns>
        public static AudioDelta AudioDelta(
            string eventId = null,
            string responseId = null,
            string itemId = null,
            int outputIndex = 0,
            int contentIndex = 0,
            string delta = null)
        {
            return new AudioDelta(eventId, responseId, itemId, outputIndex, contentIndex, delta);
        }

        /// <summary>
        ///     Creates an <see cref="AudioDelta" /> from a JSON element.
        /// </summary>
        /// <param name="element">The JSON element containing the audio delta data.</param>
        /// <returns>A new <see cref="AudioDelta" /> instance.</returns>
        public static AudioDelta AudioDeltaFromJson(JsonElement element)
        {
            return new AudioDelta(
                GetStringProperty(element, "event_id"),
                GetStringProperty(element, "response_id"),
                GetStringProperty(element, "item_id"),
                GetIntProperty(element, "output_index"),
                GetIntProperty(element, "content_index"),
                GetStringProperty(element, "delta"));
        }

        #endregion

        #region TranscriptDelta Factory Methods

        /// <summary>
        ///     Creates a new <see cref="TranscriptDelta" /> instance.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="responseId">The response identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="outputIndex">The output index.</param>
        /// <param name="contentIndex">The content index.</param>
        /// <param name="delta">The transcript text delta.</param>
        /// <returns>A new <see cref="TranscriptDelta" /> instance.</returns>
        public static TranscriptDelta TranscriptDelta(
            string eventId = null,
            string responseId = null,
            string itemId = null,
            int outputIndex = 0,
            int contentIndex = 0,
            string delta = null)
        {
            return new TranscriptDelta(eventId, responseId, itemId, outputIndex, contentIndex, delta);
        }

        /// <summary>
        ///     Creates a <see cref="TranscriptDelta" /> from a JSON element.
        /// </summary>
        /// <param name="element">The JSON element containing the transcript delta data.</param>
        /// <returns>A new <see cref="TranscriptDelta" /> instance.</returns>
        public static TranscriptDelta TranscriptDeltaFromJson(JsonElement element)
        {
            return new TranscriptDelta(
                GetStringProperty(element, "event_id"),
                GetStringProperty(element, "response_id"),
                GetStringProperty(element, "item_id"),
                GetIntProperty(element, "output_index"),
                GetIntProperty(element, "content_index"),
                GetStringProperty(element, "delta"));
        }

        #endregion

        #region TranscriptionResult Factory Methods

        /// <summary>
        ///     Creates a new <see cref="TranscriptionResult" /> instance.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <param name="contentIndex">The content index.</param>
        /// <param name="transcript">The complete transcript text.</param>
        /// <returns>A new <see cref="TranscriptionResult" /> instance.</returns>
        public static TranscriptionResult TranscriptionResult(
            string eventId = null,
            string itemId = null,
            int contentIndex = 0,
            string transcript = null)
        {
            return new TranscriptionResult(eventId, itemId, contentIndex, transcript);
        }

        /// <summary>
        ///     Creates a <see cref="TranscriptionResult" /> from a JSON element.
        /// </summary>
        /// <param name="element">The JSON element containing the transcription data.</param>
        /// <returns>A new <see cref="TranscriptionResult" /> instance.</returns>
        public static TranscriptionResult TranscriptionResultFromJson(JsonElement element)
        {
            return new TranscriptionResult(
                GetStringProperty(element, "event_id"),
                GetStringProperty(element, "item_id"),
                GetIntProperty(element, "content_index"),
                GetStringProperty(element, "transcript"));
        }

        #endregion

        #region SpeechStarted Factory Methods

        /// <summary>
        ///     Creates a new <see cref="SpeechStarted" /> instance.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="audioStartMs">The audio start time in milliseconds.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <returns>A new <see cref="SpeechStarted" /> instance.</returns>
        public static SpeechStarted SpeechStarted(
            string eventId = null,
            int audioStartMs = 0,
            string itemId = null)
        {
            return new SpeechStarted(eventId, audioStartMs, itemId);
        }

        /// <summary>
        ///     Creates a <see cref="SpeechStarted" /> from a JSON element.
        /// </summary>
        /// <param name="element">The JSON element containing the speech started data.</param>
        /// <returns>A new <see cref="SpeechStarted" /> instance.</returns>
        public static SpeechStarted SpeechStartedFromJson(JsonElement element)
        {
            return new SpeechStarted(
                GetStringProperty(element, "event_id"),
                GetIntProperty(element, "audio_start_ms"),
                GetStringProperty(element, "item_id"));
        }

        #endregion

        #region SpeechStopped Factory Methods

        /// <summary>
        ///     Creates a new <see cref="SpeechStopped" /> instance.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="audioEndMs">The audio end time in milliseconds.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <returns>A new <see cref="SpeechStopped" /> instance.</returns>
        public static SpeechStopped SpeechStopped(
            string eventId = null,
            int audioEndMs = 0,
            string itemId = null)
        {
            return new SpeechStopped(eventId, audioEndMs, itemId);
        }

        /// <summary>
        ///     Creates a <see cref="SpeechStopped" /> from a JSON element.
        /// </summary>
        /// <param name="element">The JSON element containing the speech stopped data.</param>
        /// <returns>A new <see cref="SpeechStopped" /> instance.</returns>
        public static SpeechStopped SpeechStoppedFromJson(JsonElement element)
        {
            return new SpeechStopped(
                GetStringProperty(element, "event_id"),
                GetIntProperty(element, "audio_end_ms"),
                GetStringProperty(element, "item_id"));
        }

        #endregion

        #region ResponseInfo Factory Methods

        /// <summary>
        ///     Creates a new <see cref="ResponseInfo" /> instance.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="id">The response identifier.</param>
        /// <param name="status">The response status.</param>
        /// <returns>A new <see cref="ResponseInfo" /> instance.</returns>
        public static ResponseInfo ResponseInfo(
            string eventId = null,
            string id = null,
            string status = null)
        {
            return new ResponseInfo(eventId, id, status);
        }

        /// <summary>
        ///     Creates a <see cref="ResponseInfo" /> from a JSON element.
        /// </summary>
        /// <param name="element">The JSON element containing the response data.</param>
        /// <returns>A new <see cref="ResponseInfo" /> instance.</returns>
        public static ResponseInfo ResponseInfoFromJson(JsonElement element)
        {
            var responseElement = element.TryGetProperty("response", out var response) ? response : element;
            return new ResponseInfo(
                GetStringProperty(element, "event_id"),
                GetStringProperty(responseElement, "id"),
                GetStringProperty(responseElement, "status"));
        }

        #endregion

        #region VoiceLiveError Factory Methods

        /// <summary>
        ///     Creates a new <see cref="VoiceLiveError" /> instance.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="errorType">The error type category.</param>
        /// <param name="param">The parameter that caused the error.</param>
        /// <returns>A new <see cref="VoiceLiveError" /> instance.</returns>
        public static VoiceLiveError VoiceLiveError(
            string eventId = null,
            string code = null,
            string message = null,
            string errorType = null,
            string param = null)
        {
            return new VoiceLiveError(eventId, code, message, errorType, param);
        }

        /// <summary>
        ///     Creates a <see cref="VoiceLiveError" /> from a JSON element.
        /// </summary>
        /// <param name="element">The JSON element containing the error data.</param>
        /// <returns>A new <see cref="VoiceLiveError" /> instance.</returns>
        public static VoiceLiveError VoiceLiveErrorFromJson(JsonElement element)
        {
            var errorElement = element.TryGetProperty("error", out var error) ? error : element;
            return new VoiceLiveError(
                GetStringProperty(element, "event_id"),
                GetStringProperty(errorElement, "code"),
                GetStringProperty(errorElement, "message"),
                GetStringProperty(errorElement, "type"),
                GetStringProperty(errorElement, "param"));
        }

        #endregion

        #region Helper Methods

        private static string GetStringProperty(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var prop))
            {
                return null;
            }

            // Handle both string and object types
            if (prop.ValueKind == JsonValueKind.String)
            {
                return prop.GetString();
            }

            // If it's an object, try to get "name" property (common pattern for voice, etc.)
            if (prop.ValueKind == JsonValueKind.Object && prop.TryGetProperty("name", out var nameProp))
            {
                return nameProp.GetString();
            }

            return null;
        }

        private static int GetIntProperty(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) && prop.TryGetInt32(out var value)
                ? value
                : 0;
        }

        #endregion
    }
}