// <PackageReference Include="WebSocketSharp" Version="1.0.3-rc11" />
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSocketSharp;
using WebSocketSharp.Server;


namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Signals
{


    public class SignalingBehavior : WebSocketBehavior
    {

        protected override void OnOpen()
        {
        }

        protected override void OnMessage(MessageEventArgs e)
        {
        }

        protected override void OnClose(CloseEventArgs e)
        {
        }
    }

    public static class SignalingServer
    {
        public static void Main()
        {
            var wssv = new WebSocketServer("ws://0.0.0.0:8080");
            wssv.AddWebSocketService<SignalingBehavior>("/signal");
            wssv.Start();
            Console.WriteLine("Signaling on ws://localhost:8080/signal?room=demo");
            Console.ReadLine();
            wssv.Stop();
        }
    }

    public sealed class SignalingClient : IDisposable
    {
        private readonly WebSocket _ws;

        public SignalingClient(string url)
        {
            _ws = new WebSocket(url);
        }

        public void Connect()
        {
            _ws.Connect();
        }

        public void OnMessage(Action<string> handler)
        {
            _ws.OnMessage += (s, e) => { if (e.IsText) handler(e.Data); };
        }

        public void SignalSend(object payload)
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });
            _ws.Send(json);
        }

        public void Dispose()
        {
            if (_ws != null) _ws.Close();
        }
    }

}
