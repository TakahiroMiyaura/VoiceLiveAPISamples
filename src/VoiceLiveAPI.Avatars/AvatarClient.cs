// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars.Clients.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Microsoft.Extensions.Logging;
using SIPSorcery;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars
{
    /// <summary>
    ///     Avatar Video Frame Received delegate
    /// </summary>
    public delegate void VideoFrameReceivedDelegate(IPEndPoint remote, uint ssrc, byte[] frame, VideoFormat fmt);


    /// <summary>
    ///     Provides functionality to manage avatar video and audio streams,
    ///     handle connection setup, and process received media frames for the VoiceLiveAPI.
    /// </summary>
    public class AvatarClient : ILogOutputClass
    {
        private RTCPeerConnection pc;

        /// <summary>
        ///     Provides logging functionality for the AvatarClient class.
        /// </summary>
        public ILogger Logger => LoggerFactoryManager.CreateLogger<AvatarClient>();

        /// <summary>
        ///     Avatar Video Frame Received Event.
        /// </summary>
        public event VideoFrameReceivedDelegate OnVideoFrameReceived;

        /// <summary>
        ///     Send avatar connect message.
        /// </summary>
        /// <param name="server">ICE server information.</param>
        /// <param name="client">Instance of VoiceLiveAPI client.</param>
        public async Task AvatarConnectAsync(IceServers server, VoiceLiveAPIClientBase client)
        {
            LogFactory.Set(LoggerFactoryManager.Current);
            var cfg = new RTCConfiguration
            {
                iceServers = new List<RTCIceServer>
                {
                    new RTCIceServer
                    {
                        urls = server.Urls[0],
                        username = server.UserName,
                        credential = server.Credential
                    }
                },
                // DTLS 実装相性回避で RSA を使う（必要な環境で）
                X_UseRsaForDtlsCertificate = false
            };
            pc = new RTCPeerConnection(cfg);

            var h264 = new SDPAudioVideoMediaFormat(SDPMediaTypesEnum.video, 96, "H264/90000",
                "packetization-mode=1;profile-level-id=42e01f");
            pc.addTrack(new MediaStreamTrack(
                SDPMediaTypesEnum.video, /*isLocal*/ false,
                new List<SDPAudioVideoMediaFormat> { h264 },
                MediaStreamStatusEnum.RecvOnly));

            pc.AcceptRtpFromAny = true;

            var tcs = new TaskCompletionSource<bool>();
            pc.onicegatheringstatechange += s =>
            {
                if (s == RTCIceGatheringState.complete) tcs.TrySetResult(true);
            };

            pc.createOffer();
            await tcs.Task;

            var fullOffer = pc.createOffer();
            await pc.setLocalDescription(fullOffer);

            SetLogProc();

            var sdp = pc.localDescription.sdp.ToString();
            sdp = sdp.Replace("UDP/TLS/RTP/SAVP", "UDP/TLS/RTP/SAVPF");
            Logger.LogDebug("{sdp}", sdp);
            sdp = sdp.Replace("\r", "\\r").Replace("\n", "\\n");
            sdp = $"{{\"type\": \"offer\",\"sdp\": \"{sdp}\"}}";

            var data = new SessionAvatarConnect
            {
                ClientSdp = Convert.ToBase64String(Encoding.UTF8.GetBytes(sdp))
            };
            await data.SendAsync(client);
        }

        /// <summary>
        ///     Handles avatar connecting message by setting remote SDP description.
        /// </summary>
        /// <param name="sdp">The SDP message to process.</param>
        public void AvatarConnecting(string sdp)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(sdp);
            var str = dict["sdp"].ToString()?.Replace("\\r\\n", "\r\n");
            Logger.Log(LogLevel.Information, "{sdp}", str);
            pc.setRemoteDescription(new RTCSessionDescriptionInit
            {
                sdp = str,
                type = RTCSdpType.answer
            });
        }

        private void SetLogProc()
        {
            pc.OnRtpPacketReceived += (ep, kind, pkt) =>
                Logger.LogDebug(
                    "[Avatar][rtp] {kind} {ep} pt={pkt.Header.PayloadType} seq={pkt.Header.SequenceNumber}", kind, ep,
                    pkt.Header.PayloadType, pkt.Header.SequenceNumber);
            pc.OnVideoFrameReceived += (ep, ssrc, frame, fmt) =>
                Logger.LogDebug("[Avatar][frame] {fmt} bytes={frame?.Length ?? 0} ssrc={ssrc}", fmt, frame?.Length ?? 0,
                    ssrc);
            pc.onconnectionstatechange += s => Console.WriteLine($"[pc] state={s}");
            pc.GetRtpChannel().OnRTPDataReceived += (media, ep, data) =>
            {
                Logger.LogDebug("[Avatar][RTP] {media} received from {ep},len={data.Length}", media, ep,
                    data.Length);
            };

            pc.onconnectionstatechange += state =>
            {
                Logger.LogDebug("[Avatar][RTCPeerConnection] state = {state}", state);
                if (state == RTCPeerConnectionState.connected)
                {
                    pc.Start();
                }
            };

            pc.OnVideoFormatsNegotiated += formats =>
            {
                var s = string.Join(", ",
                    formats.Select(f => $"{f.Codec},{f.FormatID},{f.FormatName},{f.ClockRate},{f.Parameters}"));
                Logger.LogDebug("[Avatar][video] negotiated formats = {fmt}", s);
            };

            // RTP 受信（映像/音声どちらも）
            pc.OnRtpPacketReceived += delegate(IPEndPoint remote, SDPMediaTypesEnum media, RTPPacket pkt)
            {
                var len = pkt.Payload != null ? pkt.Payload.Length : 0;
                if (media == SDPMediaTypesEnum.video)
                {
                    Logger.LogDebug("[Avatar][RTP][video] {m0} pt={m1} seq={m2} ts={m3} m={m4} len={m5}",
                        remote, pkt.Header.PayloadType, pkt.Header.SequenceNumber,
                        pkt.Header.Timestamp, pkt.Header.MarkerBit, len);
                }
                else if (media == SDPMediaTypesEnum.audio)
                {
                    Logger.LogDebug("[Avatar][RTP][audio] {m0} pt={m1} seq={m2} ts={m3} len={m4}",
                        remote, pkt.Header.PayloadType, pkt.Header.SequenceNumber,
                        pkt.Header.Timestamp, len);
                }
            };

            // 再構成済み「映像フレーム」受信（エンコード済みフレーム単位）
            pc.OnVideoFrameReceived += delegate(IPEndPoint remote, uint ssrc, byte[] frame, VideoFormat fmt)
            {
                OnVideoFrameReceived?.Invoke(remote, ssrc, frame, fmt);
                var frameLength = frame != null ? frame.Length : 0;
                var fmtStr = fmt.ToString();
                Logger.LogDebug("[Avatar][FRAME][video] {m0} ssrc={m1} format={m2} bytes={m3}",
                    remote, ssrc, fmtStr, frameLength);
            };

            // メディア別のタイムアウト（一定時間受信なし）
            pc.OnTimeout += media =>
            {
                Logger.LogDebug("[Avatar][timeout] {media} stream no packets for a while.", media);
            };
        }
    }
}