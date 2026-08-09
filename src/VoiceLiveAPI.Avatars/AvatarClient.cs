// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars
{
    /// <summary>
    ///     Avatar Video Frame Received delegate
    /// </summary>
    /// <param name="remote">Remote endpoint</param>
    /// <param name="ssrc">SSRC identifier</param>
    /// <param name="frame">Video frame data</param>
    /// <param name="fmt">Video format</param>
    /// <param name="timestamp">RTP timestamp (90000Hz clock rate for video)</param>
    public delegate void VideoFrameReceivedDelegate(IPEndPoint remote, uint ssrc, byte[] frame, VideoFormat fmt,
        uint timestamp);

    /// <summary>
    ///     Avatar Audio Frame Received delegate
    /// </summary>
    /// <param name="audioData">Audio frame data</param>
    /// <param name="timestamp">RTP timestamp (48000Hz clock rate for audio)</param>
    public delegate void AudioFrameReceivedDelegate(byte[] audioData, uint timestamp);


    /// <summary>
    ///     Provides a dependency-neutral WebRTC media client for avatar video and audio streams:
    ///     it negotiates RecvOnly H.264 / Opus tracks, creates the SDP offer, applies the server's
    ///     SDP answer, and raises events for received media frames.
    /// </summary>
    /// <remarks>
    ///     This client handles only the media plane and does not depend on any session SDK. The SDP
    ///     signaling (sending the offer / receiving the answer) is performed by the caller using either
    ///     the official <c>Azure.AI.VoiceLive</c> SDK (<c>ConnectAvatarAsync</c> /
    ///     <c>SessionUpdateAvatarConnecting</c>) or the self-made Core, then handed back via
    ///     <see cref="CreateSdpOfferStringAsync" /> and <see cref="AvatarConnecting" />.
    /// </remarks>
    public class AvatarClient
    {
        #region Private Fields

        /// <summary>
        ///     Latest audio RTP timestamp (48000Hz clock).
        /// </summary>
        private uint lastAudioTimestamp;

        /// <summary>
        ///     Latest video RTP timestamp (90000Hz clock).
        /// </summary>
        private uint lastVideoTimestamp;

        private RTCPeerConnection pc;

        #endregion

        #region Properties

        /// <summary>
        ///     Provides logging functionality for the AvatarClient class.
        /// </summary>
        public ILogger Logger { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AvatarClient" /> class.
        /// </summary>
        /// <param name="logger">The logger instance. Falls back to a no-op logger when null.</param>
        public AvatarClient(ILogger logger = null)
        {
            Logger = logger ?? NullLogger.Instance;
        }

        #endregion

        #region Events

        /// <summary>
        ///     Avatar Video Frame Received Event.
        /// </summary>
        public event VideoFrameReceivedDelegate OnVideoFrameReceived;

        /// <summary>
        ///     Avatar Audio Frame Received Event.
        /// </summary>
        public event AudioFrameReceivedDelegate OnAudioFrameReceived;

        #endregion

        #region public methods

        /// <summary>
        ///     Creates a Base64-encoded SDP offer string for use with a session API (e.g. the official
        ///     Azure.AI.VoiceLive SDK's <c>ConnectAvatarAsync</c>, or the self-made Core).
        /// </summary>
        /// <param name="server">ICE server information.</param>
        /// <returns>A Base64-encoded SDP offer string.</returns>
        public async Task<string> CreateSdpOfferStringAsync(AvatarIceServer server)
        {
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
            var pcm16 = new SDPAudioVideoMediaFormat(SDPMediaTypesEnum.audio, 111, "opus/48000/2");
            pc.addTrack(new MediaStreamTrack(
                SDPMediaTypesEnum.video, /*isLocal*/ false,
                new List<SDPAudioVideoMediaFormat> { h264 },
                MediaStreamStatusEnum.RecvOnly));
            pc.addTrack(new MediaStreamTrack(
                SDPMediaTypesEnum.audio, false,
                new List<SDPAudioVideoMediaFormat> { pcm16 },
                MediaStreamStatusEnum.RecvOnly)
            );
            pc.AcceptRtpFromAny = true;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            pc.onicegatheringstatechange += s =>
            {
                if (s == RTCIceGatheringState.complete) tcs.TrySetResult(true);
            };

            pc.createOffer();
            await tcs.Task;

            RTCSessionDescriptionInit fullOffer = pc.createOffer();
            await pc.setLocalDescription(fullOffer);

            SetLogProc();

            string sdp = pc.localDescription.sdp.ToString();
            sdp = sdp.Replace("UDP/TLS/RTP/SAVP", "UDP/TLS/RTP/SAVPF");
            Logger.LogDebug("{sdp}", sdp);
            sdp = sdp.Replace("\r", "\\r").Replace("\n", "\\n");
            sdp = $"{{\"type\": \"offer\",\"sdp\": \"{sdp}\"}}";

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(sdp));
        }

        /// <summary>
        ///     Handles avatar connecting message by setting remote SDP description.
        /// </summary>
        /// <param name="sdp">The SDP message to process.</param>
        public void AvatarConnecting(string sdp)
        {
            Dictionary<string, object> dict = JsonSerializer.Deserialize<Dictionary<string, object>>(sdp);
            string str = dict["sdp"].ToString()?.Replace("\\r\\n", "\r\n");
            Logger.LogDebug("{sdp}", str);
            pc.setRemoteDescription(new RTCSessionDescriptionInit
            {
                sdp = str,
                type = RTCSdpType.answer
            });
        }

        #endregion

        #region private methods

        private void SetLogProc()
        {
            pc.OnRtpPacketReceived += (ep, kind, pkt) =>
                Logger.LogTrace(
                    "[Avatar][rtp] {kind} {ep} pt={pkt.Header.PayloadType} seq={pkt.Header.SequenceNumber}", kind, ep,
                    pkt.Header.PayloadType, pkt.Header.SequenceNumber);
            pc.OnVideoFrameReceived += (ep, ssrc, frame, fmt) =>
                Logger.LogTrace("[Avatar][frame] {fmt} bytes={frame?.Length ?? 0} ssrc={ssrc}", fmt, frame?.Length ?? 0,
                    ssrc);
            pc.onconnectionstatechange += s => Logger.LogTrace($"[pc] state={s}");
            pc.GetRtpChannel().OnRTPDataReceived += (media, ep, data) =>
            {
                Logger.LogTrace("[Avatar][RTP] {media} received from {ep},len={data.Length}", media, ep,
                    data.Length);
            };

            pc.onconnectionstatechange += state =>
            {
                Logger.LogTrace("[Avatar][RTCPeerConnection] state = {state}", state);
                if (state == RTCPeerConnectionState.connected)
                {
                    pc.Start();
                }
            };

            pc.OnVideoFormatsNegotiated += formats =>
            {
                string s = string.Join(", ",
                    formats.Select(f => $"{f.Codec},{f.FormatID},{f.FormatName},{f.ClockRate},{f.Parameters}"));
                Logger.LogTrace("[Avatar][video] negotiated formats = {fmt}", s);
            };

            // RTP 受信（映像/音声どちらも）- タイムスタンプを保存
            pc.OnRtpPacketReceived += delegate(IPEndPoint remote, SDPMediaTypesEnum media, RTPPacket pkt)
            {
                int len = pkt.Payload != null ? pkt.Payload.Length : 0;
                if (media == SDPMediaTypesEnum.video)
                {
                    lastVideoTimestamp = pkt.Header.Timestamp;
                    Logger.LogTrace("[Avatar][RTP][video] {m0} pt={m1} seq={m2} ts={m3} m={m4} len={m5}",
                        remote, pkt.Header.PayloadType, pkt.Header.SequenceNumber,
                        pkt.Header.Timestamp, pkt.Header.MarkerBit, len);
                }
                else if (media == SDPMediaTypesEnum.audio)
                {
                    lastAudioTimestamp = pkt.Header.Timestamp;
                    Logger.LogTrace("[Avatar][RTP][audio] {m0} pt={m1} seq={m2} ts={m3} len={m4}",
                        remote, pkt.Header.PayloadType, pkt.Header.SequenceNumber,
                        pkt.Header.Timestamp, len);
                }
            };

            // 再構成済み「映像フレーム」受信（エンコード済みフレーム単位）- タイムスタンプ付きで渡す
            pc.OnVideoFrameReceived += delegate(IPEndPoint remote, uint ssrc, byte[] frame, VideoFormat fmt)
            {
                OnVideoFrameReceived?.Invoke(remote, ssrc, frame, fmt, lastVideoTimestamp);
                int frameLength = frame != null ? frame.Length : 0;
                string fmtStr = fmt.ToString();
                Logger.LogTrace("[Avatar][FRAME][video] {m0} ssrc={m1} format={m2} bytes={m3} ts={m4}",
                    remote, ssrc, fmtStr, frameLength, lastVideoTimestamp);
            };

            // 再構成済み「音声フレーム」受信（エンコード済みフレーム単位）
            int audioFrameReceivedCount = 0;
            int audioDataEmptyCount = 0;
            int audioDataValidCount = 0;

            pc.OnAudioFrameReceived += delegate(EncodedAudioFrame audioFrame)
            {
                audioFrameReceivedCount++;
                Logger.LogTrace("[Avatar][FRAME][audio] OnAudioFrameReceived called (#{count})",
                    audioFrameReceivedCount);

                // Try different possible property names for audio data
                byte[] audioData = null;

                // Common property names in audio frame classes
                if (audioFrame != null)
                {
                    Logger.LogTrace("[Avatar][FRAME][audio] audioFrame is not null, type: {type}",
                        audioFrame.GetType().Name);
                    audioData = audioFrame.EncodedAudio;
                    Logger.LogTrace(
                        "channel:{format},ClockRate:{ClockRate},Codec:{Codec},FormatID:{FormatID},FormatName{FormatName},Parameters{Parameters},RtpClockRate{RtpClockRate}",
                        audioFrame.AudioFormat.ChannelCount,
                        audioFrame.AudioFormat.ClockRate, audioFrame.AudioFormat.Codec, audioFrame.AudioFormat.FormatID,
                        audioFrame.AudioFormat.FormatName, audioFrame.AudioFormat.Parameters,
                        audioFrame.AudioFormat.RtpClockRate);
                }
                else
                {
                    Logger.LogTrace("[Avatar][FRAME][audio] audioFrame is null");
                }

                if (audioData != null && audioData.Length > 0)
                {
                    audioDataValidCount++;
                    Logger.LogTrace(
                        "[Avatar][FRAME][audio] Invoking OnAudioFrameReceived with {bytes} bytes, ts={ts}. Event subscribers: {subscribers}",
                        audioData.Length, lastAudioTimestamp, OnAudioFrameReceived?.GetInvocationList()?.Length ?? 0);

                    if (OnAudioFrameReceived != null)
                    {
                        OnAudioFrameReceived.Invoke(audioData, lastAudioTimestamp);
                        Logger.LogTrace("[Avatar][FRAME][audio] OnAudioFrameReceived event invoked successfully");
                    }
                    else
                    {
                        Logger.LogWarning("[Avatar][FRAME][audio] OnAudioFrameReceived event is null, no subscribers");
                    }
                }
                else
                {
                    audioDataEmptyCount++;
                    int dataLen = audioData?.Length ?? -1;
                    Logger.LogWarning("[Avatar][FRAME][audio] No valid audio data found in frame (data length: {len})",
                        dataLen);
                }
            };
            // メディア別のタイムアウト（一定時間受信なし）
            pc.OnTimeout += media =>
            {
                Logger.LogTrace("[Avatar][timeout] {media} stream no packets for a while.", media);
            };
        }

        #endregion
    }
}
