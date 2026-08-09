// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commands.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Windows;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.WebRtcAudio
{
    /// <summary>
    ///     Establishes a Voice Live <b>WebRTC voice</b> session: it opens the control WebSocket to
    ///     <c>/voice-live/realtime/calls</c> via the self-made Core, creates an SDP offer with a bidirectional
    ///     audio track and the <c>voice-live-events</c> data channel, exchanges SDP through
    ///     <see cref="RtcCallSdpCreate" /> / <see cref="RtcCallSdpCreated" />, and drives the peer connection
    ///     to the ICE-connected state.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Full-duplex audio is provided by a Windows <see cref="WindowsAudioEndPoint" /> (microphone
    ///         capture up / speaker playback down, Opus over RTP). The endpoint's encoded samples are sent via
    ///         <c>pc.SendAudio</c> and inbound RTP is forwarded to it, so once the peer reaches
    ///         <see cref="RTCPeerConnectionState.connected" /> the conversation audio flows both ways.
    ///     </para>
    ///     <para>
    ///         This is the WebRTC <b>voice</b> transport (audio over RTP media tracks) and is distinct from the
    ///         WebRTC <b>avatar</b> path in <c>VoiceLiveAPI.Avatars</c>. It does not use the WebSocket audio
    ///         path (<c>input_audio_buffer.append</c> / <c>response.audio.delta</c>).
    ///     </para>
    /// </remarks>
    public class VoiceLiveWebRtcCall : IDisposable
    {
        #region Private Fields

        private readonly ILogger logger;

        private RTCPeerConnection pc;
        private VoiceLiveSession session;
        private ServerMessageHandlerManager serverManager;
        private RTCDataChannel eventsChannel;
        private WindowsAudioEndPoint audioEndPoint;
        private TaskCompletionSource<bool> answerApplied;
        private bool audioStarted;
        private bool disposed;
        private long micSamplesSent;
        private long audioRtpReceived;

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the underlying peer connection state changes.
        /// </summary>
        public event Action<RTCPeerConnectionState> OnConnectionStateChanged;

        /// <summary>
        ///     Occurs when a message arrives on the <c>voice-live-events</c> data channel (VAD, transcription,
        ///     response lifecycle). The payload is the raw JSON string.
        /// </summary>
        public event Action<string> OnDataChannelMessage;

        /// <summary>
        ///     Occurs when the service reports an <c>rtc.call.error</c> for a failed call operation.
        /// </summary>
        public event Action<RtcCallError> OnCallError;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current peer connection state.
        /// </summary>
        public RTCPeerConnectionState ConnectionState =>
            pc?.connectionState ?? RTCPeerConnectionState.@new;

        /// <summary>
        ///     Gets the active control-channel session, or <c>null</c> before connecting.
        /// </summary>
        public VoiceLiveSession Session => session;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveWebRtcCall" /> class.
        /// </summary>
        /// <param name="logger">The logger instance. Falls back to a no-op logger when null.</param>
        public VoiceLiveWebRtcCall(ILogger logger = null)
        {
            this.logger = logger ?? NullLogger.Instance;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Opens the control channel, negotiates the WebRTC peer connection, and waits until the SDP answer
        ///     has been applied. After this returns, monitor <see cref="OnConnectionStateChanged" /> for the
        ///     transition to <see cref="RTCPeerConnectionState.connected" />.
        /// </summary>
        /// <param name="client">A configured Voice Live client (endpoint + credential).</param>
        /// <param name="model">The model for the <c>/calls</c> URL query (e.g. <c>azure-realtime</c>).</param>
        /// <param name="sessionConfig">
        ///     The minimal session config for the <c>rtc.call.sdp.create</c> <c>session</c> field (modalities,
        ///     instructions, voice, turn_detection). It must NOT include <c>model</c> or the WebSocket-audio
        ///     fields (<c>input_audio_format</c> / <c>input_audio_sampling_rate</c>), which make the service
        ///     fail to allocate the RTP media client.
        /// </param>
        /// <param name="iceServers">Optional ICE servers (STUN/TURN). Defaults to host candidates only.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <param name="apiKeyForQuery">
        ///     Optional API key to pass as the <c>/calls</c> URL query parameter (the WebRTC media allocation
        ///     reads it from there). Leave null to authenticate the handshake via the client's credential only.
        /// </param>
        /// <returns>A task that completes once the SDP answer is applied (or the service returns an error).</returns>
        public async Task ConnectAsync(VoiceLiveClient client, string model, object sessionConfig,
            IReadOnlyList<RTCIceServer> iceServers = null, CancellationToken cancellationToken = default,
            string apiKeyForQuery = null)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentNullException(nameof(model));
            }

            answerApplied = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Subscribe to the call signaling events BEFORE connecting so the answer/error is never missed.
            serverManager = new ServerMessageHandlerManager();
            serverManager.OnRtcCallSdpCreatedReceived += HandleSdpCreated;
            serverManager.OnRtcCallErrorReceived += HandleCallError;

            // StartCallSessionAsync uses only the model for the /calls URL and does NOT send a session.update;
            // the real session config travels in rtc.call.sdp.create's `session` field below.
            var urlOptions = VoiceLiveSessionOptions.CreateDefault();
            urlOptions.Model = model;

            session = await client
                .StartCallSessionAsync(urlOptions, new MessageHandlerManagerBase[] { serverManager },
                    cancellationToken, apiKeyForQuery)
                .ConfigureAwait(false);

            SetupPeerConnection(iceServers);

            var offerSdp = await CreateOfferSdpAsync().ConfigureAwait(false);

            logger.LogInformation("[WebRtcCall] Sending rtc.call.sdp.create ({len} chars)", offerSdp.Length);
            await session.SendMessageAsync(new RtcCallSdpCreate { SdpOffer = offerSdp, Session = sessionConfig },
                cancellationToken).ConfigureAwait(false);

            using (cancellationToken.Register(() => answerApplied.TrySetCanceled()))
            {
                await answerApplied.Task.ConfigureAwait(false);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Creates the peer connection, attaches the full-duplex audio endpoint and the
        ///     <c>voice-live-events</c> data channel, and wires the diagnostic/state callbacks.
        /// </summary>
        /// <param name="iceServers">Optional ICE servers.</param>
        private void SetupPeerConnection(IReadOnlyList<RTCIceServer> iceServers)
        {
            var config = new RTCConfiguration
            {
                iceServers = iceServers != null ? new List<RTCIceServer>(iceServers) : new List<RTCIceServer>(),
                X_UseRsaForDtlsCertificate = false
            };
            pc = new RTCPeerConnection(config);

            // Full-duplex Windows audio (mic up / speaker down). Opus is enabled on the encoder.
            audioEndPoint = new WindowsAudioEndPoint(new AudioEncoder(false, true));

            // Offer Opus ONLY. If the offer also lists telephony codecs (PCMU/PCMA/G722/G729) the service can
            // select one as the preferred codec and then fail to allocate the media client, because the
            // realtime model produces 48 kHz Opus (observed as "Remote client allocation failed").
            var opusFormats = audioEndPoint.GetAudioSourceFormats()
                .Where(f => f.Codec == AudioCodecsEnum.OPUS)
                .ToList();
            var audioTrack = new MediaStreamTrack(opusFormats, MediaStreamStatusEnum.SendRecv);
            pc.addTrack(audioTrack);
            pc.AcceptRtpFromAny = true;

            // Bind the negotiated format to both directions, pump captured samples out, and feed inbound RTP in.
            pc.OnAudioFormatsNegotiated += formats =>
            {
                if (formats == null || formats.Count == 0)
                {
                    return;
                }

                var negotiated = formats[0];
                logger.LogInformation(
                    "[WebRtcCall] Audio format negotiated: codec={codec} id={id} name={name} rate={rate}",
                    negotiated.Codec, negotiated.FormatID, negotiated.FormatName, negotiated.ClockRate);

                // The remote answer advertises lowercase "opus" on a dynamic payload, which SIPSorcery leaves
                // as Codec=Unknown — so the Windows endpoint can't encode/decode and no audio flows either way.
                // Rebuild a proper OPUS format, keeping the negotiated payload id.
                var format = negotiated.Codec != AudioCodecsEnum.OPUS &&
                             string.Equals(negotiated.FormatName, "opus", StringComparison.OrdinalIgnoreCase)
                    ? new AudioFormat(AudioCodecsEnum.OPUS, negotiated.FormatID, 48000, 2, negotiated.Parameters)
                    : negotiated;

                logger.LogInformation("[WebRtcCall] Audio format applied: codec={codec} id={id} rate={rate}",
                    format.Codec, format.FormatID, format.ClockRate);
                audioEndPoint.SetAudioSourceFormat(format);
                audioEndPoint.SetAudioSinkFormat(format);
            };

            // Diagnostics: count microphone-encoded samples sent and audio RTP packets received so a silent
            // direction is obvious (mic not capturing vs. no server audio).
            audioEndPoint.OnAudioSourceEncodedSample += (durationRtpUnits, sample) =>
            {
                micSamplesSent++;
                if (micSamplesSent == 1 || micSamplesSent % 100 == 0)
                {
                    logger.LogInformation("[WebRtcCall] mic encoded samples sent: {n} (last {bytes} bytes)",
                        micSamplesSent, sample?.Length ?? 0);
                }

                pc.SendAudio(durationRtpUnits, sample);
            };

            pc.OnRtpPacketReceived += (remote, media, pkt) =>
            {
                if (media == SDPMediaTypesEnum.audio && pkt?.Payload != null)
                {
                    audioRtpReceived++;
                    if (audioRtpReceived == 1 || audioRtpReceived % 100 == 0)
                    {
                        logger.LogInformation("[WebRtcCall] audio RTP received: {n} (pt={pt})",
                            audioRtpReceived, pkt.Header.PayloadType);
                    }

                    // GotAudioRtp derives frame timing from the RTP header internally. The newer
                    // GotEncodedMediaFrame requires a pre-built EncodedAudioFrame (format + duration), which we
                    // avoid here to not hand-compute frame durations.
#pragma warning disable CS0618
                    audioEndPoint.GotAudioRtp(remote, pkt.Header.SyncSource, pkt.Header.SequenceNumber,
                        pkt.Header.Timestamp, pkt.Header.PayloadType, pkt.Header.MarkerBit == 1, pkt.Payload);
#pragma warning restore CS0618
                }
            };

            // The data channel that carries VAD / transcription / response lifecycle events.
            _ = pc.createDataChannel("voice-live-events").ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully && t.Result != null)
                {
                    AttachEventsChannel(t.Result);
                }
            });

            // The service may instead open the data channel from its side.
            pc.ondatachannel += AttachEventsChannel;

            pc.onconnectionstatechange += state =>
            {
                logger.LogInformation("[WebRtcCall] Peer connection state = {state}", state);
                OnConnectionStateChanged?.Invoke(state);
                _ = HandleConnectionStateAsync(state);
            };

            pc.oniceconnectionstatechange += state =>
                logger.LogDebug("[WebRtcCall] ICE connection state = {state}", state);
            pc.onicegatheringstatechange += state =>
                logger.LogDebug("[WebRtcCall] ICE gathering state = {state}", state);
        }

        /// <summary>
        ///     Starts audio capture/playback when connected and stops it on teardown.
        /// </summary>
        /// <param name="state">The new peer connection state.</param>
        private async Task HandleConnectionStateAsync(RTCPeerConnectionState state)
        {
            try
            {
                if (state == RTCPeerConnectionState.connected)
                {
                    await pc.Start().ConfigureAwait(false);
                    if (!audioStarted)
                    {
                        audioStarted = true;
                        // StartAudio starts the microphone (source); the speaker (sink) is started separately,
                        // otherwise received RTP is decoded but never played.
                        await audioEndPoint.StartAudio().ConfigureAwait(false);
                        await audioEndPoint.StartAudioSink().ConfigureAwait(false);
                        logger.LogInformation("[WebRtcCall] Audio started (mic up / speaker down)");
                    }
                }
                else if (state == RTCPeerConnectionState.closed ||
                         state == RTCPeerConnectionState.failed ||
                         state == RTCPeerConnectionState.disconnected)
                {
                    if (audioStarted)
                    {
                        audioStarted = false;
                        await audioEndPoint.CloseAudio().ConfigureAwait(false);
                        await audioEndPoint.CloseAudioSink().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[WebRtcCall] Error handling connection state {state}", state);
            }
        }

        /// <summary>
        ///     Runs ICE gathering and returns the normalized local SDP offer to send as <c>sdp_offer</c>.
        /// </summary>
        /// <returns>The local SDP offer text.</returns>
        private async Task<string> CreateOfferSdpAsync()
        {
            // setLocalDescription is what starts ICE gathering, so it must come BEFORE awaiting completion
            // (otherwise gathering never starts and the wait deadlocks).
            var offer = pc.createOffer();
            await pc.setLocalDescription(offer).ConfigureAwait(false);

            // Wait (bounded) for gathering to complete so host/srflx candidates are embedded in the SDP.
            // Guard both races: gathering may already be complete (the event won't fire again), or it may
            // never complete (proceed with the host candidates already gathered).
            if (pc.iceGatheringState != RTCIceGatheringState.complete)
            {
                var gatheringComplete =
                    new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                void OnGathering(RTCIceGatheringState s)
                {
                    if (s == RTCIceGatheringState.complete)
                    {
                        gatheringComplete.TrySetResult(true);
                    }
                }

                pc.onicegatheringstatechange += OnGathering;
                if (pc.iceGatheringState == RTCIceGatheringState.complete)
                {
                    gatheringComplete.TrySetResult(true);
                }

                await Task.WhenAny(gatheringComplete.Task, Task.Delay(2000)).ConfigureAwait(false);
                pc.onicegatheringstatechange -= OnGathering;
            }

            var sdp = pc.localDescription.sdp.ToString();
            // Advertise SAVPF (RTCP feedback) to match what the service expects.
            sdp = sdp.Replace("UDP/TLS/RTP/SAVP", "UDP/TLS/RTP/SAVPF");
            sdp = AugmentAudioOfferForVoiceLive(sdp);
            logger.LogDebug("[WebRtcCall] Local SDP offer:\n{sdp}", sdp);
            return sdp;
        }

        /// <summary>
        ///     Adds the audio-media attributes that the Voice Live WebRTC media server needs to allocate its
        ///     media client, which SIPSorcery's minimal offer omits: transport-wide congestion control feedback
        ///     (<c>transport-cc</c> + its header extension), the standard header extensions, reduced-size RTCP,
        ///     and an <c>msid</c> binding. Without these the service returns "Remote client allocation failed"
        ///     (verified against Chrome's working offer, which includes them).
        /// </summary>
        /// <param name="sdp">The local SDP offer.</param>
        /// <returns>The augmented SDP offer.</returns>
        private static string AugmentAudioOfferForVoiceLive(string sdp)
        {
            const string streamId = "voicelive-stream";
            const string trackId = "voicelive-audio";

            var lines = sdp.Replace("\r\n", "\n").TrimEnd('\n').Split('\n').ToList();

            // SIPSorcery leaves the default connection address as 0.0.0.0:9. Browsers set it to their primary
            // host candidate, and the media server appears to reject the null address when allocating. Point
            // m=/c= at the first IPv4 UDP host candidate.
            string defaultIp = null;
            string defaultPort = null;
            foreach (var l in lines)
            {
                if (l.StartsWith("a=candidate:") && l.Contains(" udp ") && l.Contains(" typ host"))
                {
                    var p = l.Split(' ');
                    if (p.Length >= 6 && p[4].Contains("."))
                    {
                        defaultIp = p[4];
                        defaultPort = p[5];
                        break;
                    }
                }
            }

            for (int i = 0; i < lines.Count; i++)
            {
                if (defaultIp != null && lines[i] == "c=IN IP4 0.0.0.0")
                {
                    lines[i] = "c=IN IP4 " + defaultIp;
                }
                else if (defaultPort != null && (lines[i].StartsWith("m=audio ") || lines[i].StartsWith("m=application ")))
                {
                    var p = lines[i].Split(' ');
                    p[1] = defaultPort;
                    lines[i] = string.Join(" ", p);
                }
                else if (lines[i] == "s=sipsorcery")
                {
                    // Browsers use a bare session name.
                    lines[i] = "s=-";
                }
                else if (lines[i].StartsWith("a=ice-options:"))
                {
                    // Chrome offers plain "trickle"; SIPSorcery adds "ice2".
                    lines[i] = "a=ice-options:trickle";
                }
            }

            // Session-level: stream semantics + allow mixed one/two-byte header extensions.
            int groupIdx = lines.FindIndex(l => l.StartsWith("a=group:BUNDLE"));
            if (groupIdx >= 0 && !lines.Any(l => l.StartsWith("a=msid-semantic")))
            {
                lines.Insert(groupIdx + 1, "a=msid-semantic: WMS " + streamId);
                lines.Insert(groupIdx + 1, "a=extmap-allow-mixed");
            }

            // Extract the audio SSRC SIPSorcery generated so we can add the msid binding for it.
            var ssrc = "0";
            var ssrcLine = lines.FirstOrDefault(l => l.StartsWith("a=ssrc:") && l.Contains("cname:"));
            if (ssrcLine != null)
            {
                ssrc = ssrcLine.Substring("a=ssrc:".Length).Split(' ')[0];
            }

            // Insert (not replace) the attributes the str0m media server needs, WITHOUT touching SIPSorcery's
            // codec block: the payload list / rtpmap must stay as SIPSorcery generated them, otherwise its
            // internal state desyncs from the sent SDP and the negotiated codec resolves to "Unknown" (no audio).
            var result = new List<string>();
            var inAudio = false;
            foreach (var line in lines)
            {
                if (line.StartsWith("m="))
                {
                    inAudio = line.StartsWith("m=audio");
                }

                if (!inAudio)
                {
                    result.Add(line);
                    continue;
                }

                if (line.StartsWith("a=fmtp:111"))
                {
                    // Opus config to match Chrome, then the congestion-control feedback the server requires.
                    result.Add("a=fmtp:111 minptime=10;useinbandfec=1");
                    result.Add("a=rtcp-fb:111 transport-cc");
                    continue;
                }

                result.Add(line);

                if (line.StartsWith("a=mid:"))
                {
                    result.Add("a=extmap:1 urn:ietf:params:rtp-hdrext:ssrc-audio-level");
                    result.Add("a=extmap:2 http://www.webrtc.org/experiments/rtp-hdrext/abs-send-time");
                    result.Add(
                        "a=extmap:3 http://www.ietf.org/id/draft-holmer-rmcat-transport-wide-cc-extensions-01");
                    result.Add("a=extmap:4 urn:ietf:params:rtp-hdrext:sdes:mid");
                }
                else if (line.StartsWith("a=rtcp-mux"))
                {
                    result.Add("a=rtcp-rsize");
                }
                else if (line == "a=sendrecv")
                {
                    result.Add("a=msid:" + streamId + " " + trackId);
                }
                else if (line.StartsWith("a=ssrc:") && line.Contains("cname:"))
                {
                    result.Add("a=ssrc:" + ssrc + " msid:" + streamId + " " + trackId);
                }
            }

            return string.Join("\r\n", result) + "\r\n";
        }

        /// <summary>
        ///     Applies the SDP answer from <c>rtc.call.sdp.created</c> as the remote description.
        /// </summary>
        /// <param name="created">The SDP-created event.</param>
        private void HandleSdpCreated(RtcCallSdpCreated created)
        {
            try
            {
                logger.LogInformation("[WebRtcCall] Received rtc.call.sdp.created (rtc_call_id={id})",
                    created.RtcCallId);
                var result = pc.setRemoteDescription(new RTCSessionDescriptionInit
                {
                    sdp = created.SdpAnswer,
                    type = RTCSdpType.answer
                });
                logger.LogInformation("[WebRtcCall] setRemoteDescription result = {result}", result);
                answerApplied?.TrySetResult(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[WebRtcCall] Failed to apply SDP answer");
                answerApplied?.TrySetException(ex);
            }
        }

        /// <summary>
        ///     Surfaces an <c>rtc.call.error</c> and fails the pending connect.
        /// </summary>
        /// <param name="error">The error event.</param>
        private void HandleCallError(RtcCallError error)
        {
            logger.LogError("[WebRtcCall] rtc.call.error: operation={op} type={type} code={code} message={message}",
                error.Operation, error.Error?.Type, error.Error?.Code, error.Error?.Message);
            OnCallError?.Invoke(error);
            answerApplied?.TrySetException(
                new InvalidOperationException($"rtc.call.error: {error.Error?.Code} {error.Error?.Message}"));
        }

        /// <summary>
        ///     Wires the <c>voice-live-events</c> data channel message callback.
        /// </summary>
        /// <param name="channel">The data channel.</param>
        private void AttachEventsChannel(RTCDataChannel channel)
        {
            if (channel == null)
            {
                return;
            }

            eventsChannel = channel;
            logger.LogInformation("[WebRtcCall] Data channel '{label}' attached", channel.label);
            channel.onmessage += (dc, protocol, data) =>
            {
                var text = data != null ? System.Text.Encoding.UTF8.GetString(data) : string.Empty;
                logger.LogDebug("[WebRtcCall] data-channel message: {text}", text);
                OnDataChannelMessage?.Invoke(text);
            };
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        ///     Releases the peer connection and control-channel resources.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            try
            {
                _ = audioEndPoint?.CloseAudio();
            }
            catch (Exception ex)
            {
                logger.LogTrace(ex, "[WebRtcCall] Error closing audio endpoint");
            }

            try
            {
                eventsChannel?.close();
            }
            catch (Exception ex)
            {
                logger.LogTrace(ex, "[WebRtcCall] Error closing data channel");
            }

            try
            {
                pc?.close();
            }
            catch (Exception ex)
            {
                logger.LogTrace(ex, "[WebRtcCall] Error closing peer connection");
            }

            try
            {
                session?.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogTrace(ex, "[WebRtcCall] Error disposing session");
            }
        }

        #endregion
    }
}
