// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveSDK
{
    /// <summary>
    ///     Subscribes to the Azure.AI.VoiceLive SDK's OpenTelemetry distributed tracing (beta.4) and
    ///     surfaces per-operation diagnostics (token usage, latency, turn/interruption counts, etc.).
    /// </summary>
    /// <remarks>
    ///     The SDK emits spans via a <see cref="System.Diagnostics.ActivitySource" /> named
    ///     <c>"Azure.AI.VoiceLive"</c> and only produces them while a listener is attached
    ///     (<c>ActivitySource.HasListeners()</c>). No extra instrumentation package is required: this
    ///     helper registers a plain <see cref="ActivityListener" /> and prints the GenAI semantic
    ///     convention attributes (<c>gen_ai.*</c>) of each completed span.
    /// </remarks>
    internal static class VoiceLiveTelemetry
    {
        #region Static Fields and Constants

        /// <summary>The ActivitySource name emitted by the Azure.AI.VoiceLive SDK.</summary>
        private const string SourceName = "Azure.AI.VoiceLive";

        #endregion

        #region public methods

        /// <summary>
        ///     Registers an <see cref="ActivityListener" /> for the Voice Live SDK's tracing source.
        ///     Completed spans are written to the console (always visible) and traced via the logger.
        /// </summary>
        /// <param name="logger">The logger used for detailed (trace-level) output.</param>
        /// <returns>The registered listener; dispose it to stop listening.</returns>
        public static IDisposable Enable(ILogger logger)
        {
            var listener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == SourceName,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                SampleUsingParentId =
                    (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = activity => OnActivityStopped(activity, logger)
            };

            ActivitySource.AddActivityListener(listener);
            Console.WriteLine("[OTel] Voice Live tracing enabled (source: Azure.AI.VoiceLive)");
            return listener;
        }

        #endregion

        #region private methods

        /// <summary>
        ///     Writes a compact one-line summary of a completed span (name, duration and GenAI attributes).
        /// </summary>
        private static void OnActivityStopped(Activity activity, ILogger logger)
        {
            string genAiTags = string.Join(", ", activity.TagObjects
                .Where(tag => tag.Key.StartsWith("gen_ai.", StringComparison.Ordinal))
                .Select(tag => $"{tag.Key}={tag.Value}"));

            string summary =
                $"[OTel] {activity.OperationName} ({activity.Duration.TotalMilliseconds:F0}ms)" +
                (genAiTags.Length > 0 ? $" {genAiTags}" : string.Empty);

            Console.WriteLine(summary);
            logger?.LogTrace("{summary}", summary);
        }

        #endregion
    }
}
