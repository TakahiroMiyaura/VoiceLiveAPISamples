// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.Text.Json;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Events
{
    /// <summary>
    ///     Factory class for creating ServerEvent instances from JSON messages.
    /// </summary>
    /// <remarks>
    ///     This factory parses incoming JSON messages and creates the appropriate
    ///     strongly-typed <see cref="ServerEvent" /> subclass based on the message type.
    /// </remarks>
    public static class ServerEventFactory
    {
        #region Static Fields and Constants

        /// <summary>
        ///     Registry of event type to factory function mappings.
        /// </summary>
        private static readonly Dictionary<string, Func<JsonElement, ServerEvent>> Factories =
            new Dictionary<string, Func<JsonElement, ServerEvent>>();

        /// <summary>
        ///     Flag indicating whether the factory has been initialized.
        /// </summary>
        private static bool initialized;

        /// <summary>
        ///     Lock object for thread-safe initialization.
        /// </summary>
        private static readonly object InitLock = new object();

        #endregion

        #region Public Methods

        /// <summary>
        ///     Creates a ServerEvent from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string containing the event data.</param>
        /// <returns>A ServerEvent instance, or null if the JSON cannot be parsed.</returns>
        public static ServerEvent FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
#if NET8_0_OR_GREATER
                using var document = JsonDocument.Parse(json);
#else
                var document = JsonDocument.Parse(json);
#endif
                return FromJsonElement(document.RootElement);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        ///     Creates a ServerEvent from a JsonElement.
        /// </summary>
        /// <param name="element">The JSON element containing the event data.</param>
        /// <returns>A ServerEvent instance, or null if the type is not recognized.</returns>
        public static ServerEvent FromJsonElement(JsonElement element)
        {
            EnsureInitialized();

            if (!element.TryGetProperty("type", out var typeProperty))
            {
                return null;
            }

            var type = typeProperty.GetString();
            if (type == null || !Factories.TryGetValue(type, out var factory))
            {
                return null;
            }

            return factory(element);
        }

        /// <summary>
        ///     Registers a factory function for a specific event type.
        /// </summary>
        /// <param name="eventType">The event type string identifier.</param>
        /// <param name="factory">The factory function that creates the event from JSON.</param>
        public static void Register(string eventType, Func<JsonElement, ServerEvent> factory)
        {
            Factories[eventType] = factory;
        }

        /// <summary>
        ///     Checks if a factory is registered for the specified event type.
        /// </summary>
        /// <param name="eventType">The event type string identifier.</param>
        /// <returns>True if a factory is registered, false otherwise.</returns>
        public static bool IsRegistered(string eventType)
        {
            EnsureInitialized();
            return eventType != null && Factories.ContainsKey(eventType);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Ensures the factory is initialized with default factories.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            lock (InitLock)
            {
                if (initialized)
                {
                    return;
                }

                RegisterDefaultFactories();
                initialized = true;
            }
        }

        /// <summary>
        ///     Registers the default factory functions for all known event types.
        /// </summary>
        private static void RegisterDefaultFactories()
        {
            // Audio events
            Register(AudioDelta.TypeName, VoiceLiveModelFactory.AudioDeltaFromJson);
            Register(TranscriptDelta.TypeName, VoiceLiveModelFactory.TranscriptDeltaFromJson);

            // Speech events
            Register(SpeechStarted.TypeName, VoiceLiveModelFactory.SpeechStartedFromJson);
            Register(SpeechStopped.TypeName, VoiceLiveModelFactory.SpeechStoppedFromJson);

            // Session events
            Register(SessionInfo.TypeNameCreated, VoiceLiveModelFactory.SessionInfoFromJson);
            Register(SessionInfo.TypeNameUpdated, VoiceLiveModelFactory.SessionInfoFromJson);

            // Response events
            Register(ResponseInfo.TypeName, VoiceLiveModelFactory.ResponseInfoFromJson);

            // Error event
            Register(VoiceLiveError.TypeName, VoiceLiveModelFactory.VoiceLiveErrorFromJson);

            // Transcription events
            Register(TranscriptionResult.TypeName, VoiceLiveModelFactory.TranscriptionResultFromJson);

            // Function call events
            Register(FunctionCallDelta.TypeName, VoiceLiveModelFactory.FunctionCallDeltaFromJson);
            Register(FunctionCallDone.TypeName, VoiceLiveModelFactory.FunctionCallDoneFromJson);

            // Foundry Agent call events (2026-01-01-preview)
            Register(FoundryAgentCallArgumentsDelta.TypeName,
                VoiceLiveModelFactory.FoundryAgentCallArgumentsDeltaFromJson);
            Register(FoundryAgentCallArgumentsDone.TypeName,
                VoiceLiveModelFactory.FoundryAgentCallArgumentsDoneFromJson);
            Register(FoundryAgentCallInProgress.TypeName,
                VoiceLiveModelFactory.FoundryAgentCallInProgressFromJson);
            Register(FoundryAgentCallCompleted.TypeName,
                VoiceLiveModelFactory.FoundryAgentCallCompletedFromJson);
            Register(FoundryAgentCallFailed.TypeName, VoiceLiveModelFactory.FoundryAgentCallFailedFromJson);

            // TODO: Add factory methods for remaining event types
            // - response.audio.done
            // - response.audio_transcript.done
            // - input_audio_buffer.committed
            // - input_audio_buffer.cleared
            // - response.created
            // - response.output_item.added
            // - response.output_item.done
            // - response.content_part.added
            // - response.content_part.done
            // - conversation.item.input_audio_transcription.failed
            // - conversation.created
            // - conversation.item.created
            // - conversation.item.deleted
            // - conversation.item.retrieved
            // - conversation.item.truncated
            // - response.text.delta
            // - response.text.done
            // - session.avatar.connecting
            // - response.animation.viseme.delta
            // - response.animation.viseme.done
            // - output_audio_buffer.started
            // - output_audio_buffer.stopped
            // - output_audio_buffer.cleared
            // - rate_limits.updated
        }

        #endregion
    }
}