// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.SessionUpdates;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core
{
    /// <summary>
    ///     Represents a WebSocket-based session for real-time voice communication with Azure AI VoiceLive service.
    /// </summary>
    /// <remarks>
    ///     This class manages the WebSocket connection lifecycle, message sending/receiving,
    ///     and provides event-based notifications for incoming messages.
    ///     Implements <see cref="IAsyncDisposable" /> for proper resource cleanup.
    /// </remarks>
    public class VoiceLiveSession : IAsyncDisposable, IDisposable, ILogOutputClass
    {
        #region Private Fields

        private readonly ClientWebSocket webSocket;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Queue<string> receivedMessages;
        private readonly List<MessageHandlerManagerBase> managers;
        private readonly ConcurrentQueue<byte[]> audioOutputQueue;
        private readonly SemaphoreSlim sendSemaphore;
        private readonly JsonSerializerOptions jsonSerializerOptions;
        private readonly Channel<SessionUpdate> sessionUpdateChannel;
        private readonly Channel<string> rawMessageChannel;

        private Task receiveTask;
        private Task messageHandlingTask;
        private bool disposed;
        private bool enableSessionUpdates;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the WebSocket connection URI.
        /// </summary>
        public Uri ConnectionUri { get; }

        /// <summary>
        ///     Gets the session options.
        /// </summary>
        public VoiceLiveSessionOptions Options { get; }

        /// <summary>
        ///     Gets or sets the logger instance.
        /// </summary>
        public ILogger Logger { get; set; } = LoggerFactoryManager.CreateLogger<VoiceLiveSession>();

        /// <summary>
        ///     Gets the current state of the WebSocket connection.
        /// </summary>
        public WebSocketState State => webSocket?.State ?? WebSocketState.None;

        /// <summary>
        ///     Gets a value indicating whether the session is connected.
        /// </summary>
        public bool IsConnected => webSocket?.State == WebSocketState.Open;

        /// <summary>
        ///     Gets the number of audio chunks available in the output queue.
        /// </summary>
        public int AudioQueueCount => audioOutputQueue.Count;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveSession" /> class.
        /// </summary>
        /// <param name="connectionUri">The WebSocket URI to connect to.</param>
        /// <param name="options">The session options.</param>
        internal VoiceLiveSession(Uri connectionUri, VoiceLiveSessionOptions options)
        {
            ConnectionUri = connectionUri ?? throw new ArgumentNullException(nameof(connectionUri));
            Options = options ?? throw new ArgumentNullException(nameof(options));

            webSocket = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();
            receivedMessages = new Queue<string>();
            managers = new List<MessageHandlerManagerBase>();
            audioOutputQueue = new ConcurrentQueue<byte[]>();
            sendSemaphore = new SemaphoreSlim(1, 1);
            receiveTask = Task.CompletedTask;
            messageHandlingTask = Task.CompletedTask;

            // Initialize channels for SessionUpdate streaming
            sessionUpdateChannel = Channel.CreateUnbounded<SessionUpdate>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true
            });
            rawMessageChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true
            });

            jsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Adds a message handler manager to process incoming messages.
        /// </summary>
        /// <param name="manager">The message handler manager to add.</param>
        public void AddMessageHandlerManager(MessageHandlerManagerBase manager)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (!managers.Contains(manager))
            {
                managers.Add(manager);
            }
        }

        /// <summary>
        ///     Removes a message handler manager.
        /// </summary>
        /// <param name="manager">The message handler manager to remove.</param>
        public void RemoveMessageHandlerManager(MessageHandlerManagerBase manager)
        {
            managers.Remove(manager);
        }

        /// <summary>
        ///     Sends input audio data to the session.
        /// </summary>
        /// <param name="audioData">The audio data as a byte array.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendInputAudioAsync(byte[] audioData, CancellationToken cancellationToken = default)
        {
            if (audioData == null || audioData.Length == 0) return;

            var base64Audio = Convert.ToBase64String(audioData);
            var message = new
            {
                type = "input_audio_buffer.append",
                audio = base64Audio
            };

            await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Sends input audio data from a stream to the session.
        /// </summary>
        /// <param name="audioStream">The audio data stream.</param>
        /// <param name="chunkSize">The size of each chunk to send. Defaults to 16384 bytes.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendInputAudioAsync(Stream audioStream, int chunkSize = 16384,
            CancellationToken cancellationToken = default)
        {
            if (audioStream == null) throw new ArgumentNullException(nameof(audioStream));

            var buffer = new byte[chunkSize];
            int bytesRead;

            while ((bytesRead = await audioStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                       .ConfigureAwait(false)) > 0)
            {
                var chunk = new byte[bytesRead];
                Array.Copy(buffer, chunk, bytesRead);
                await SendInputAudioAsync(chunk, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Commits the current input audio buffer.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CommitInputAudioAsync(CancellationToken cancellationToken = default)
        {
            var message = new { type = "input_audio_buffer.commit" };
            await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Clears the input audio buffer.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ClearInputAudioAsync(CancellationToken cancellationToken = default)
        {
            var message = new { type = "input_audio_buffer.clear" };
            await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Clears the output streaming audio buffer on the server.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        ///     This method sends a request to clear the server-side output audio buffer,
        ///     which stops any audio that is currently being streamed.
        ///     To clear the local audio queue, use <see cref="ClearAudioQueue" /> instead.
        /// </remarks>
        public async Task ClearStreamingAudioAsync(CancellationToken cancellationToken = default)
        {
            var message = new { type = "output_audio_buffer.clear" };
            await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Cancels the current response.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CancelResponseAsync(CancellationToken cancellationToken = default)
        {
            var message = new { type = "response.cancel" };
            await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Updates the session configuration.
        /// </summary>
        /// <param name="options">The new session options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ConfigureSessionAsync(VoiceLiveSessionOptions options,
            CancellationToken cancellationToken = default)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var message = new
            {
                type = "session.update",
                session = options
            };

            // Log the session.update content for debugging (use Information level for visibility)
            var json = System.Text.Json.JsonSerializer.Serialize(message, jsonSerializerOptions);
            Logger?.LogInformation("Sending session.update (Avatar: {hasAvatar})", options.Avatar != null ? "configured" : "null");
            Logger?.LogDebug("session.update JSON: {json}", json);

            await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
            Logger?.LogInformation("session.update sent successfully");
        }

        /// <summary>
        ///     Sends a user text message to the session.
        /// </summary>
        /// <param name="text">The text message to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendUserMessageAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            var message = new
            {
                type = "conversation.item.create",
                item = new
                {
                    type = "message",
                    role = "user",
                    content = new[]
                    {
                        new
                        {
                            type = "input_text", text
                        }
                    }
                }
            };
            await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Sends a function call result to the session.
        /// </summary>
        /// <param name="callId">The function call ID.</param>
        /// <param name="result">The function result as a string.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendFunctionResultAsync(string callId, string result,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(callId)) throw new ArgumentNullException(nameof(callId));

            var message = new
            {
                type = "conversation.item.create",
                item = new
                {
                    type = "function_call_output",
                    call_id = callId,
                    output = result ?? string.Empty
                }
            };
            await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Triggers response generation.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreateResponseAsync(CancellationToken cancellationToken = default)
        {
            var message = new { type = "response.create" };
            await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Sends a raw message to the session.
        /// </summary>
        /// <param name="message">The message object to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendMessageAsync(object message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            await sendSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var json = JsonSerializer.Serialize(message, jsonSerializerOptions);
                var buffer = Encoding.UTF8.GetBytes(json);

                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                sendSemaphore.Release();
            }
        }

        /// <summary>
        ///     Clears all audio data from the output queue.
        /// </summary>
        public void ClearAudioQueue()
        {
            while (audioOutputQueue.TryDequeue(out _))
            {
            }
        }

        /// <summary>
        ///     Tries to dequeue an audio chunk from the output queue.
        /// </summary>
        /// <param name="audioData">The dequeued audio data.</param>
        /// <returns><c>true</c> if an audio chunk was dequeued; otherwise, <c>false</c>.</returns>
        public bool TryDequeueAudio(out byte[] audioData)
        {
            return audioOutputQueue.TryDequeue(out audioData);
        }

        /// <summary>
        ///     Enqueues audio data to the output queue.
        /// </summary>
        /// <param name="audioData">The audio data to enqueue.</param>
        public void EnqueueAudio(byte[] audioData)
        {
            if (audioData != null && audioData.Length > 0)
            {
                audioOutputQueue.Enqueue(audioData);
            }
        }

        /// <summary>
        ///     Gets an asynchronous enumerable of session updates.
        ///     This method implements the modern IAsyncEnumerable pattern for processing messages.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>An async enumerable of session updates.</returns>
        /// <remarks>
        ///     When using this method, the session will convert incoming messages to SessionUpdate objects.
        ///     This is the preferred way to handle messages in the new API pattern.
        /// </remarks>
        /// <example>
        ///     <code>
        /// await foreach (var update in session.GetUpdatesAsync())
        /// {
        ///     switch (update)
        ///     {
        ///         case SessionUpdateResponseAudioDelta audio:
        ///             // Handle audio data
        ///             break;
        ///         case SessionUpdateError error:
        ///             // Handle error
        ///             break;
        ///     }
        /// }
        /// </code>
        /// </example>
        public async IAsyncEnumerable<SessionUpdate> GetUpdatesAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            enableSessionUpdates = true;

            var reader = sessionUpdateChannel.Reader;
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, cancellationTokenSource.Token);

            try
            {
                while (await reader.WaitToReadAsync(linkedCts.Token).ConfigureAwait(false))
                {
                    while (reader.TryRead(out var update))
                    {
                        yield return update;
                    }
                }
            }
            finally
            {
                linkedCts.Dispose();
            }
        }

        /// <summary>
        ///     Enables session update processing.
        ///     When enabled, received messages are converted to SessionUpdate objects.
        /// </summary>
        public void EnableSessionUpdates()
        {
            enableSessionUpdates = true;
        }

        /// <summary>
        ///     Tries to read a session update from the channel.
        /// </summary>
        /// <param name="update">The session update if available.</param>
        /// <returns>True if an update was available, false otherwise.</returns>
        public bool TryReadSessionUpdate(out SessionUpdate update)
        {
            return sessionUpdateChannel.Reader.TryRead(out update);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Connects to the WebSocket server and starts the session.
        /// </summary>
        /// <param name="setupAuthentication">A function to set up authentication headers.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task ConnectAsync(Func<ClientWebSocket, Task> setupAuthentication,
            CancellationToken cancellationToken = default)
        {
            Logger?.LogInformation("ConnectAsync starting - Handlers registered: {count}", managers.Count);

            if (setupAuthentication != null)
            {
                await setupAuthentication(webSocket).ConfigureAwait(false);
            }

            Logger?.LogInformation("Connecting to: {uri}", ConnectionUri);

            await webSocket.ConnectAsync(ConnectionUri, cancellationToken).ConfigureAwait(false);

            Logger?.LogInformation("WebSocket connected, state: {state}", webSocket.State);

            // Send session configuration
            Logger?.LogInformation("Sending initial session.update...");
            await ConfigureSessionAsync(Options, cancellationToken).ConfigureAwait(false);

            // Start receive loop
            Logger?.LogInformation("Starting receive and message handling loops...");
            receiveTask = ReceiveLoopAsync();
            messageHandlingTask = MessageHandlingLoopAsync();

            Logger?.LogInformation("Session connected successfully, waiting for server events...");
        }

        #endregion

        #region Private Methods

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[4096];
            var messageBuffer = new StringBuilder();

            try
            {
                while (webSocket.State == WebSocketState.Open &&
                       !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cancellationTokenSource.Token).ConfigureAwait(false);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageChunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuffer.Append(messageChunk);

                        if (result.EndOfMessage)
                        {
                            var completeMessage = messageBuffer.ToString();
                            messageBuffer.Clear();
                            lock (receivedMessages)
                            {
                                receivedMessages.Enqueue(completeMessage);
                            }
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Logger?.LogInformation("WebSocket close received");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Receive loop error: {message}", ex.Message);
            }
        }

        private async Task MessageHandlingLoopAsync()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                string json = null;
                lock (receivedMessages)
                {
                    if (receivedMessages.Count > 0)
                    {
                        json = receivedMessages.Dequeue();
                    }
                }

                if (json == null)
                {
                    await Task.Delay(10, cancellationTokenSource.Token).ConfigureAwait(false);
                    continue;
                }

                try
                {
                    var document = JsonDocument.Parse(json);
                    var root = document.RootElement;

                    if (!root.TryGetProperty("type", out var typeElement))
                    {
                        Logger?.LogWarning("Message has no type field");
                        continue;
                    }

                    var messageType = typeElement.GetString();
                    if (string.IsNullOrEmpty(messageType))
                    {
                        Logger?.LogWarning("Message type is null or empty");
                        continue;
                    }

                    // Use Information level for session events to ensure visibility
                    if (messageType.StartsWith("session.") || messageType.StartsWith("error"))
                    {
                        Logger?.LogInformation("Received message type: {type}", messageType);
                    }
                    else
                    {
                        Logger?.LogDebug("Processing message type: {type}", messageType);
                    }

                    // Process with traditional handlers
                    var handlerFound = false;
                    foreach (var manager in managers)
                    {
                        if (manager.GetRegisteredHandlers().TryGetValue(messageType, out var handler))
                        {
                            handlerFound = true;
                            Logger?.LogDebug("Handler found for message type: {type}", messageType);
                            try
                            {
                                await handler.HandleAsync(root).ConfigureAwait(false);
                            }
                            catch (Exception handlerEx)
                            {
                                Logger?.LogError(handlerEx, "Handler exception for message type: {type}", messageType);
                            }
                        }
                    }

                    if (!handlerFound)
                    {
                        Logger?.LogDebug("No handler registered for message type: {type}", messageType);
                    }

                    // If SessionUpdates are enabled, convert and write to channel
                    if (enableSessionUpdates)
                    {
                        await WriteSessionUpdateAsync(messageType, root).ConfigureAwait(false);
                    }
                }
                catch (JsonException ex)
                {
                    Logger?.LogError(ex, "Failed to parse JSON message");
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Failed to process message");
                }
            }

            // Complete the channel when done
            sessionUpdateChannel.Writer.TryComplete();
        }

        private async Task WriteSessionUpdateAsync(string messageType, JsonElement root)
        {
            try
            {
                // Create a basic MessageBase for the factory
                var messageBase = new MessageBase
                {
                    Type = messageType,
                    EventId = root.TryGetProperty("event_id", out var eventIdElement)
                        ? eventIdElement.GetString()
                        : null
                };

                // Create the appropriate SessionUpdate based on type
                var sessionUpdate = CreateSessionUpdate(messageType, root, messageBase);

                if (sessionUpdate != null)
                {
                    await sessionUpdateChannel.Writer.WriteAsync(sessionUpdate, cancellationTokenSource.Token)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(ex, "Failed to create SessionUpdate for type: {type}", messageType);
            }
        }

        private SessionUpdate CreateSessionUpdate(string messageType, JsonElement root, MessageBase messageBase)
        {
            switch (messageType)
            {
                case SessionUpdateSessionCreated.TypeName:
                    return new SessionUpdateSessionCreated(messageBase, null);

                case SessionUpdateSessionUpdated.TypeName:
                    return new SessionUpdateSessionUpdated(messageBase, null);

                case SessionUpdateResponseAudioDelta.TypeName:
                    return new SessionUpdateResponseAudioDelta(
                        messageBase,
                        GetStringProperty(root, "response_id"),
                        GetStringProperty(root, "item_id"),
                        GetIntProperty(root, "output_index"),
                        GetIntProperty(root, "content_index"),
                        GetStringProperty(root, "delta"));

                case SessionUpdateError.TypeName:
                    var error = root.TryGetProperty("error", out var errorElement) ? errorElement : root;
                    return new SessionUpdateError(
                        messageBase,
                        GetStringProperty(error, "code"),
                        GetStringProperty(error, "message"),
                        GetStringProperty(error, "type"));

                case SessionUpdateInputAudioBufferSpeechStarted.TypeName:
                    return new SessionUpdateInputAudioBufferSpeechStarted(
                        messageBase,
                        GetIntProperty(root, "audio_start_ms"),
                        GetStringProperty(root, "item_id"));

                case SessionUpdateInputAudioBufferSpeechStopped.TypeName:
                    return new SessionUpdateInputAudioBufferSpeechStopped(
                        messageBase,
                        GetIntProperty(root, "audio_end_ms"),
                        GetStringProperty(root, "item_id"));

                case SessionUpdateResponseAudioTranscriptDelta.TypeName:
                    return new SessionUpdateResponseAudioTranscriptDelta(
                        messageBase,
                        GetStringProperty(root, "response_id"),
                        GetStringProperty(root, "item_id"),
                        GetIntProperty(root, "output_index"),
                        GetIntProperty(root, "content_index"),
                        GetStringProperty(root, "delta"));

                case SessionUpdateResponseDone.TypeName:
                    var response = root.TryGetProperty("response", out var responseElement)
                        ? responseElement
                        : root;
                    return new SessionUpdateResponseDone(
                        messageBase,
                        GetStringProperty(response, "id"),
                        GetStringProperty(response, "status"));

                case SessionUpdateConversationItemCreated.TypeName:
                    return new SessionUpdateConversationItemCreated(
                        messageBase,
                        GetStringProperty(root, "previous_item_id"),
                        null);

                case SessionUpdateTranscriptionCompleted.TypeName:
                    return new SessionUpdateTranscriptionCompleted(
                        messageBase,
                        GetStringProperty(root, "item_id"),
                        GetIntProperty(root, "content_index"),
                        GetStringProperty(root, "transcript"));

                case SessionUpdateResponseOutputItemDone.TypeName:
                    return new SessionUpdateResponseOutputItemDone(
                        messageBase,
                        GetStringProperty(root, "response_id"),
                        GetIntProperty(root, "output_index"),
                        null);

                default:
                    // Return unknown update for unrecognized types
                    return new SessionUpdateUnknown(messageBase);
            }
        }

        private static string GetStringProperty(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
        }

        private static int GetIntProperty(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) && prop.TryGetInt32(out var value)
                ? value
                : 0;
        }

        #endregion

        #region IAsyncDisposable Implementation

        /// <summary>
        ///     Asynchronously releases resources used by the session.
        /// </summary>
        /// <returns>A task representing the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (disposed) return;

            cancellationTokenSource.Cancel();

            if (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Session disposed",
                        CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger?.LogWarning(ex, "Error closing WebSocket");
                }
            }

            try
            {
                await Task.WhenAll(receiveTask, messageHandlingTask).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }

            webSocket.Dispose();
            cancellationTokenSource.Dispose();
            sendSemaphore.Dispose();

            disposed = true;

            GC.SuppressFinalize(this);
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        ///     Releases resources used by the session.
        /// </summary>
        public void Dispose()
        {
            DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        #endregion
    }
}