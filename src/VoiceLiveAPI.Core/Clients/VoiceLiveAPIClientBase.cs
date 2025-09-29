// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients
{
    /// <summary>
    ///     Abstract base class for Azure AI VoiceInfo Live API clients.
    ///     Provides common functionality for WebSocket communication, audio processing, and message handling.
    /// </summary>
    public abstract class VoiceLiveAPIClientBase : IDisposable, ILogOutputClass
    {
        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the VoiceLiveAPIClientBase.
        /// </summary>
        protected VoiceLiveAPIClientBase()
        {
            WebSocket = new ClientWebSocket();
            CancellationTokenSource = new CancellationTokenSource();
            ReceiveTask = Task.CompletedTask;
            messageHandlingTask = Task.CompletedTask;
            AccessToken = string.Empty;
        }

        #endregion


        #region Private Fields

        private readonly Queue<string> receivedMessages = new Queue<string>();

        private Task messageHandlingTask;

        /// <summary>
        ///     The WebSocket client used for communication.
        /// </summary>
        protected ClientWebSocket WebSocket;

        /// <summary>
        ///     The cancellation token source for managing task cancellation.
        /// </summary>
        protected CancellationTokenSource CancellationTokenSource;

        /// <summary>
        ///     A queue for storing audio output data.
        /// </summary>
        protected readonly ConcurrentQueue<byte[]> AudioOutputQueue = new ConcurrentQueue<byte[]>();

        /// <summary>
        ///     Dictionary for storing registered message handlers.
        /// </summary>
        private readonly Dictionary<string, IVoiceLiveHandler> messageHandlers =
            new Dictionary<string, IVoiceLiveHandler>();

        /// <summary>
        ///     The task responsible for receiving data.
        /// </summary>
        protected Task ReceiveTask;

        /// <summary>
        ///     The access token used for authentication.
        /// </summary>
        protected string AccessToken;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the Azure AI endpoint URL.
        /// </summary>
        public string Endpoint { get; protected set; } = string.Empty;

        /// <summary>
        ///     Gets the API version.
        /// </summary>
        public string ApiVersion { get; protected set; } = "2025-05-01-preview";


        /// <summary>
        ///     Gets or sets the logger instance.
        /// </summary>
        public abstract ILogger Logger { set; get; }

        #endregion


        #region Public Methods

        private readonly List<MessageHandlerManagerBase> managers = new List<MessageHandlerManagerBase>();
        private static JsonSerializerOptions jsonSerializerOptions;

        /// <summary>
        ///     Registers a <see cref="MessageHandlerManagerBase" /> to handle incoming messages.
        ///     If the manager is already registered, this method does nothing.
        /// </summary>
        /// <param name="manager">The message handler manager to add.</param>
        public void AddMessageHandlerManager(MessageHandlerManagerBase manager)
        {
            if (managers.Contains(manager)) return;
            managers.Add(manager);
        }

        /// <summary>
        ///     Unregisters the specified <see cref="MessageHandlerManagerBase" />.
        ///     If the manager is not registered, this method does nothing.
        /// </summary>
        /// <param name="manager">The message handler manager to remove.</param>
        public void RemoveMessageHandlerManager(MessageHandlerManagerBase manager)
        {
            managers.Remove(manager);
        }

        /// <summary>
        ///     Clears all audio data from the output queue.
        /// </summary>
        public virtual void ClearAudioQueue()
        {
            while (AudioOutputQueue.TryDequeue(out _))
            {
            }
        }

        /// <summary>
        ///     Establishes a WebSocket connection to the VoiceInfo Live API.
        /// </summary>
        /// <returns>A task representing the asynchronous connect operation.</returns>
        public virtual async Task ConnectAsync(IClientSessionUpdate sessionUpdated)
        {
            try
            {
                // Setup authentication
                await SetupAuthenticationAsync();

                var uri = BuildConnectionUri();

                Log(LogLevel.Information, $"Connecting to: {uri}");
                Log(LogLevel.Information, "Auth Method: Token-based");

                await WebSocket.ConnectAsync(new Uri(uri), CancellationTokenSource.Token);

                await SendMessageAsync(sessionUpdated);

                ReceiveTask = ReceiveLoop();
                messageHandlingTask = MessageHandlingTask();
            }
            catch (Exception ex)
            {
                LogError($"Connection failed: {ex.Message}", ex);
                throw;
            }
        }


        /// <summary>
        ///     Disconnects from the VoiceInfo Live API and cleans up resources.
        /// </summary>
        /// <returns>A task representing the asynchronous disconnect operation.</returns>
        public virtual async Task DisconnectAsync()
        {
            try
            {
                CancellationTokenSource.Cancel();

                if (WebSocket.State == WebSocketState.Open)
                {
                    await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }

                await ReceiveTask;
                await messageHandlingTask;
            }
            catch (Exception ex)
            {
                LogError($"Disconnection failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Gets the number of audio chunks available in the output queue.
        /// </summary>
        /// <returns>The number of audio chunks in the queue.</returns>
        public virtual int GetAudioQueueCount()
        {
            return AudioOutputQueue.Count;
        }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        ///     Builds the WebSocket connection URI specific to the implementation.
        /// </summary>
        /// <returns>The connection URI string.</returns>
        protected abstract string BuildConnectionUri();

        /// <summary>
        ///     Sets up authentication specific to the implementation.
        /// </summary>
        /// <returns>A task representing the asynchronous authentication setup.</returns>
        protected abstract Task SetupAuthenticationAsync();

        /// <summary>
        ///     Registers a message handler for a specific message type.
        /// </summary>
        /// <param name="handler">The message handler to register.</param>
        protected void RegisterMessageHandler(IVoiceLiveHandler handler)
        {
            messageHandlers[handler.MessageType] = handler;
            Log(LogLevel.Debug, $"Registered handler for message type: {handler.MessageType}");
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Logs a message for debugging or informational purposes.
        /// </summary>
        /// <param name="level">Specifies the log level.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="maxMessageLength">The maximum length of the message (default is 50).</param>
        protected virtual void Log(LogLevel level, string message, int maxMessageLength = 50)
        {
            var msg = string.Concat(
                message.Substring(0, message.Length > maxMessageLength ? maxMessageLength : message.Length), "...");
            Logger?.Log(level, "{msg}", msg);
        }

        /// <summary>
        ///     Sends a message asynchronously.
        /// </summary>
        /// <param name="message">The message object to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual async Task SendMessageAsync(object message)
        {
            jsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var json = JsonSerializer.Serialize(message, jsonSerializerOptions);
            var buffer = Encoding.UTF8.GetBytes(json);

            await WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                CancellationTokenSource.Token);
        }

        /// <summary>
        ///     Sends a message to the server asynchronously.
        /// </summary>
        /// <param name="message">The message object to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendServerAsync(object message)
        {
            await SendMessageAsync(message);
        }

        /// <summary>
        ///     Logs an error message along with an exception.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="exception">The exception associated with the error.</param>
        protected virtual void LogError(string message, Exception exception = null)
        {
            Logger?.LogError(exception, "{message}", message);
        }

        #endregion

        #region Private Methods

        private async Task ReceiveLoop()
        {
            var buffer = new byte[1024];
            var messageBuffer = new StringBuilder();

            try
            {
                while (WebSocket.State == WebSocketState.Open &&
                       !CancellationTokenSource.Token.IsCancellationRequested)
                {
                    var result =
                        await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageChunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuffer.Append(messageChunk);

                        if (result.EndOfMessage)
                        {
                            var completeMessage = messageBuffer.ToString();
                            messageBuffer.Clear();
                            lock (receivedMessages)
                                receivedMessages.Enqueue(completeMessage);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex) when (!CancellationTokenSource.Token.IsCancellationRequested)
            {
                LogError($"Receive loop error: {ex.Message}", ex);
            }
        }

        private async Task MessageHandlingTask()
        {
            while (true)
            {
                var count = 0;
                lock (receivedMessages)
                {
                    count = receivedMessages.Count;
                }

                if (count == 0)
                {
                    await Task.Delay(100);
                    continue;
                }

                var json = "";
                lock (receivedMessages)
                {
                    json = receivedMessages.Dequeue();
                }

                try
                {
                    var document = JsonDocument.Parse(json);
                    var root = document.RootElement;

                    if (!root.TryGetProperty("type", out var typeElement))
                    {
                        LogError("Message has no type field");
                        return;
                    }

                    var messageType = typeElement.GetString();
                    if (string.IsNullOrEmpty(messageType))
                    {
                        LogError("Message type is null or empty");
                        return;
                    }

                    Log(LogLevel.Information, $"Process message type:{messageType}");
                    Log(LogLevel.Trace, $"message:{root}");
                    foreach (var manager in managers)
                    {
                        // Try to handle with registered handlers first
                        if (manager.GetRegisteredHandlers().TryGetValue(messageType, out var handler))
                        {
                            await handler.HandleAsync(root);
                        }
                    }
                }
                catch (JsonException ex)
                {
                    LogError($"Failed to parse JSON message: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    LogError($"Failed to process message: {ex.Message}", ex);
                }
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        ///     Releases all resources used by the VoiceLiveAPIClientBase.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases resources used by the VoiceLiveAPIClientBase.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancellationTokenSource.Cancel();
                WebSocket.Dispose();
                CancellationTokenSource.Dispose();
            }
        }

        #endregion
    }
}