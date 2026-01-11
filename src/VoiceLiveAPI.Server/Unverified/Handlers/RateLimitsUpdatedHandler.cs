// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Messages;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Server.Unverified.Handlers
{
    /// <summary>
    ///     Handles rate limits updated messages.
    /// </summary>
    public class RateLimitsUpdatedHandler : VoiceLiveHandlerBase<RateLimitsUpdatedMessage>
    {
        /// <summary>
        ///     Gets the event type for rate limits updated.
        /// </summary>
        public const string EventType = "rate_limits.updated";

        /// <summary>
        ///     Gets the message type this handler can process.
        /// </summary>
        public override string MessageType => EventType;

        /// <summary>
        ///     Gets or sets the <see cref="ILogger" /> used for logging output in this handler.
        /// </summary>
        public override ILogger Logger { set; get; } = LoggerFactoryManager.CreateLogger<RateLimitsUpdatedHandler>();

        /// <summary>
        ///     Occurs when a rate limits updated message is processed.
        /// </summary>
        public override event Action<RateLimitsUpdatedMessage> OnProcessMessage;

        /// <summary>
        ///     Handles the rate limits updated message asynchronously.
        /// </summary>
        /// <param name="message">The JSON message to handle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task HandleAsync(JsonElement message)
        {
            var messageString = message.GetRawText();
            var rateLimitsUpdated = JsonSerializer.Deserialize<RateLimitsUpdatedMessage>(messageString);
            using (var writer = new StreamWriter(new FileStream(EventType + ".txt", FileMode.Create)))
            {
                await writer.WriteAsync(messageString);
                await writer.FlushAsync();
            }

            if (rateLimitsUpdated == null)
                throw new InvalidOperationException("Deserialization failed for RateLimitsUpdatedMessage.");
            OnProcessMessage?.Invoke(rateLimitsUpdated);
            await Task.CompletedTask;
        }
    }
}