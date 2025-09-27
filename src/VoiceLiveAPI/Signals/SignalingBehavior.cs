// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Signals
{
    /// <summary>
    /// WebSocket behavior implementation for handling signaling communication in avatar video streaming.
    /// </summary>
    public class SignalingBehavior : WebSocketBehavior
    {
        #region Protected methods

        /// <summary>
        /// Called when a WebSocket connection is opened.
        /// </summary>
        protected override void OnOpen()
        {
        }

        /// <summary>
        /// Called when a message is received from the WebSocket client.
        /// </summary>
        /// <param name="e">The message event arguments containing the received data.</param>
        protected override void OnMessage(MessageEventArgs e)
        {
        }

        /// <summary>
        /// Called when the WebSocket connection is closed.
        /// </summary>
        /// <param name="e">The close event arguments containing closure details.</param>
        protected override void OnClose(CloseEventArgs e)
        {
        }

        #endregion
    }

    /// <summary>
    /// WebSocket server for handling signaling communication in avatar video streaming scenarios.
    /// </summary>
    public static class SignalingServer
    {
        #region Public methods

        /// <summary>
        /// Starts the signaling server and listens for WebSocket connections.
        /// </summary>
        public static void Main()
        {
            var wssv = new WebSocketServer("ws://0.0.0.0:8080");
            wssv.AddWebSocketService<SignalingBehavior>("/signal");
            wssv.Start();
            Console.WriteLine("Signaling on ws://localhost:8080/signal?room=demo");
            Console.ReadLine();
            wssv.Stop();
        }

        #endregion
    }

    /// <summary>
    /// WebSocket client for connecting to signaling servers in avatar video streaming scenarios.
    /// </summary>
    public sealed class SignalingClient : IDisposable
    {
        #region Private Fields

        /// <summary>
        /// The underlying WebSocket connection.
        /// </summary>
        private readonly WebSocket _ws;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalingClient"/> class.
        /// </summary>
        /// <param name="url">The WebSocket server URL to connect to.</param>
        public SignalingClient(string url)
        {
            _ws = new WebSocket(url);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Establishes a connection to the signaling server.
        /// </summary>
        public void Connect()
        {
            _ws.Connect();
        }

        /// <summary>
        /// Registers a message handler for incoming text messages.
        /// </summary>
        /// <param name="handler">The action to invoke when a text message is received.</param>
        public void OnMessage(Action<string> handler)
        {
            _ws.OnMessage += (s, e) => { if (e.IsText) handler(e.Data); };
        }

        /// <summary>
        /// Sends a serialized object as a JSON message to the signaling server.
        /// </summary>
        /// <param name="payload">The object to serialize and send.</param>
        public void SignalSend(object payload)
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });
            _ws.Send(json);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="SignalingClient"/>.
        /// </summary>
        public void Dispose()
        {
            if (_ws != null) _ws.Close();
        }

        #endregion
    }
}
