// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{
    /// <summary>
    ///     The console's microphone and speakers: capture, playback, and the barge-in handling that decides
    ///     which response's audio is still wanted.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The only thing this needs from the outside is somewhere to send captured audio, supplied as
    ///         <see cref="SendAudioAsync" />. Everything else — the buffers, the recording state, the echo
    ///         reference — is internal, so the session code no longer has to know how audio is produced or
    ///         consumed.
    ///     </para>
    ///     <para>
    ///         <b>Playback buffering.</b> Response audio arrives much faster than real time, so a long answer
    ///         queues far more than its playback duration. A short buffer silently discarded the overflow,
    ///         which truncated long answers mid-sentence — and made the echo reference diverge from what was
    ///         actually played. The buffer is therefore sized to hold any single response intact.
    ///     </para>
    /// </remarks>
    public sealed class AudioPipeline : IDisposable
    {
        #region Static Fields and Constants

        /// <summary>How often the echo-reference statistics line is printed, in milliseconds.</summary>
        private const int EcStatsIntervalMs = 5000;

        #endregion

        #region Private Fields

        /// <summary>Guards the recording state so start and stop cannot interleave.</summary>
        private readonly object recordingLock = new object();

        /// <summary>The logger, or null to stay quiet.</summary>
        private ILogger? logger;

        /// <summary>The capture sample rate, in hertz.</summary>
        private readonly int sampleRate;

        /// <summary>Bits per sample for capture and playback.</summary>
        private readonly int bitsPerSample;

        /// <summary>Channel count for capture and the standard playback path.</summary>
        private readonly int channels;

        /// <summary>How many seconds of assistant audio the playback buffer can hold.</summary>
        private readonly int playbackBufferSeconds;

        /// <summary>The microphone.</summary>
        private WaveInEvent? waveIn;

        /// <summary>The speakers.</summary>
        private WaveOutEvent? waveOut;

        /// <summary>The 24 kHz mono buffer the assistant's audio is played from.</summary>
        private BufferedWaveProvider? waveProvider;

        /// <summary>The 48 kHz stereo buffer used by the WebRTC avatar path.</summary>
        private BufferedWaveProvider? avatarWaveProvider;

        /// <summary>Builds the interleaved mic+playback frames for the client-side EC reference feature.</summary>
        private EchoReferenceStereoCapture? echoReference;

        /// <summary>The response whose audio is currently being played.</summary>
        private string? activeResponseId;

        /// <summary>The response whose audio is being dropped after an interruption.</summary>
        private string? suppressedResponseId;

        /// <summary>When the echo-reference statistics line was last printed.</summary>
        private int lastEcStatsTick;

        #endregion

        #region Properties

        /// <summary>
        ///     Sets the logger. The pipeline is constructed before logging is configured, so this is handed
        ///     over once it is.
        /// </summary>
        public ILogger? Logger
        {
            set => logger = value;
        }

        /// <summary>Gets a value indicating whether the microphone is capturing.</summary>
        public bool IsRecording { get; private set; }

        /// <summary>Gets a value indicating whether the speakers are playing.</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        ///     Gets or sets the sink for captured audio. Set this before recording starts; without it the
        ///     capture is simply discarded.
        /// </summary>
        public Func<byte[], Task>? SendAudioAsync { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether captured audio is sent as interleaved stereo
        ///     (mic on channel 0, played audio on channel 1) for the client-side EC reference feature.
        /// </summary>
        public bool UseStereoEchoReference { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the assistant's PCM is played locally. The WebRTC avatar
        ///     carries its own audio over the media stream, so the local path stays silent there.
        /// </summary>
        public bool PlayResponseAudioLocally { get; set; } = true;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AudioPipeline" /> class.
        /// </summary>
        /// <param name="sampleRate">The capture and playback sample rate, in hertz.</param>
        /// <param name="bitsPerSample">Bits per sample.</param>
        /// <param name="channels">Channel count for capture and standard playback.</param>
        /// <param name="playbackBufferSeconds">How many seconds of assistant audio the buffer holds.</param>
        /// <param name="logger">The logger, or null.</param>
        public AudioPipeline(int sampleRate, int bitsPerSample, int channels, int playbackBufferSeconds,
            ILogger? logger)
        {
            this.sampleRate = sampleRate;
            this.bitsPerSample = bitsPerSample;
            this.channels = channels;
            this.playbackBufferSeconds = playbackBufferSeconds;
            this.logger = logger;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Opens the microphone and speakers.
        /// </summary>
        /// <param name="avatarSampleRate">The avatar audio sample rate, in hertz.</param>
        /// <param name="avatarChannels">The avatar audio channel count.</param>
        /// <param name="useAvatarOutput">
        ///     Whether output runs at the avatar's format. True only for the WebRTC avatar, whose audio arrives
        ///     as 48 kHz stereo; every other mode plays the standard PCM response path.
        /// </param>
        public void Initialize(int avatarSampleRate, int avatarChannels, bool useAvatarOutput)
        {
            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(sampleRate, bitsPerSample, channels),
                BufferMilliseconds = 100
            };
            waveIn.DataAvailable += OnAudioDataAvailable;
            waveIn.RecordingStopped += OnRecordingStopped;

            waveOut = new WaveOutEvent();

            waveProvider = new BufferedWaveProvider(new WaveFormat(sampleRate, bitsPerSample, channels))
            {
                BufferLength = playbackBufferSeconds * sampleRate * channels * 2,
                DiscardOnBufferOverflow = true
            };

            if (useAvatarOutput)
            {
                avatarWaveProvider =
                    new BufferedWaveProvider(new WaveFormat(avatarSampleRate, bitsPerSample, avatarChannels))
                    {
                        BufferLength = avatarSampleRate * avatarChannels * 2 * 10,
                        DiscardOnBufferOverflow = true
                    };

                waveOut.Init(avatarWaveProvider);
                logger?.LogInformation("Audio initialized for avatar output: {rate}Hz, {channels} channels",
                    avatarSampleRate, avatarChannels);
            }
            else
            {
                waveOut.Init(waveProvider);
                logger?.LogInformation("Audio initialized: {rate}Hz, {channels} channels", sampleRate, channels);
            }
        }

        /// <summary>
        ///     Enables the client-side echo cancellation reference, which sends the mic and the audio actually
        ///     leaving the speaker as one interleaved stereo stream.
        /// </summary>
        /// <param name="capture">The reference builder, or null to turn the feature off.</param>
        public void UseEchoReference(EchoReferenceStereoCapture? capture)
        {
            echoReference = capture;
            UseStereoEchoReference = capture != null;
        }

        /// <summary>
        ///     Queues one <c>response.audio.delta</c> for playback, dropping it when it belongs to a response
        ///     that was interrupted.
        /// </summary>
        /// <param name="responseId">The response the audio belongs to; may be null.</param>
        /// <param name="pcm">The decoded PCM.</param>
        public void EnqueueResponseAudio(string? responseId, byte[] pcm)
        {
            // Barge-in gating: after an interruption the interrupted response's id is suppressed, so drop its
            // late deltas. A delta carrying a different (new) response id supersedes the interrupted one —
            // adopt it as active and stop suppressing.
            if (!string.IsNullOrEmpty(responseId))
            {
                if (responseId == suppressedResponseId)
                {
                    return;
                }

                if (responseId != activeResponseId)
                {
                    activeResponseId = responseId;
                    suppressedResponseId = null;
                }
            }

            if (!PlayResponseAudioLocally || pcm.Length == 0 || waveProvider == null || waveOut == null)
            {
                return;
            }

            // Feed the same PCM to the echo reference so the mic path can interleave what is being played.
            if (UseStereoEchoReference)
            {
                echoReference?.EnqueueReference(pcm);
            }

            lock (waveProvider)
            {
                waveProvider.AddSamples(pcm, 0, pcm.Length);
            }

            // Check the actual playback state rather than the flag: NAudio may have stopped on an empty buffer.
            if (waveOut.PlaybackState != PlaybackState.Playing)
            {
                waveOut.Play();
                IsPlaying = true;
            }
        }

        /// <summary>
        ///     Starts capturing.
        /// </summary>
        public void StartRecording()
        {
            lock (recordingLock)
            {
                if (IsRecording)
                {
                    Console.WriteLine("Already recording");
                    return;
                }

                try
                {
                    Console.WriteLine("Starting microphone...");
                    waveIn?.StartRecording();
                    IsRecording = true;
                    Console.WriteLine(
                        "🎤 Recording Start - Stops automatically when you finish speaking (Manual stop:'R' key)");
                }
                catch (Exception ex)
                {
                    logger?.LogError("Error starting recording: {Message}", ex.Message);
                }
            }
        }

        /// <summary>
        ///     Stops capturing.
        /// </summary>
        public void StopRecording()
        {
            lock (recordingLock)
            {
                if (!IsRecording)
                {
                    return;
                }

                try
                {
                    waveIn?.StopRecording();
                    IsRecording = false;
                    Console.WriteLine("Recording stopped");
                }
                catch (Exception ex)
                {
                    logger?.LogError("Error stopping recording: {Message}", ex.Message);
                }
            }
        }

        /// <summary>
        ///     Starts or stops capturing.
        /// </summary>
        public void ToggleRecording()
        {
            if (IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        /// <summary>
        ///     Starts playback.
        /// </summary>
        public void StartPlayback()
        {
            if (waveOut == null || waveOut.PlaybackState == PlaybackState.Playing)
            {
                return;
            }

            try
            {
                waveOut.Play();
                IsPlaying = true;
                Console.WriteLine("Playback started");
            }
            catch (Exception ex)
            {
                logger?.LogError("Error starting playback: {Message}", ex.Message);
            }
        }

        /// <summary>
        ///     Stops playback.
        /// </summary>
        public void StopPlayback()
        {
            // waveOut is null when audio was never initialized (WebRTC voice uses its own endpoint).
            if (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
            {
                try
                {
                    waveOut.Stop();
                    Console.WriteLine("Playback stopped");
                }
                catch (Exception ex)
                {
                    logger?.LogError("Error stopping playback: {Message}", ex.Message);
                }
            }

            IsPlaying = false;
        }

        /// <summary>
        ///     Starts or stops playback.
        /// </summary>
        public void TogglePlayback()
        {
            if (IsPlaying)
            {
                StopPlayback();
            }
            else
            {
                StartPlayback();
            }
        }

        /// <summary>
        ///     Interrupts playback on barge-in or auto-truncation: suppresses the response being played, so its
        ///     in-flight deltas are dropped, and flushes what is already queued. The next response's deltas
        ///     carry a new id and are adopted automatically.
        /// </summary>
        public void Interrupt()
        {
            suppressedResponseId = activeResponseId;
            ClearPlaybackBuffer();

            // Drop stale reference audio so the reference channel does not desync after the interruption.
            echoReference?.Reset();
        }

        /// <summary>
        ///     Drops any audio already queued for the speakers.
        /// </summary>
        public void ClearPlaybackBuffer()
        {
            if (waveProvider != null)
            {
                lock (waveProvider)
                {
                    waveProvider.ClearBuffer();
                }
            }

            avatarWaveProvider?.ClearBuffer();
        }

        /// <summary>
        ///     Clears the avatar audio buffer, used when leaving avatar mode.
        /// </summary>
        public void ReleaseAvatarBuffer()
        {
            if (avatarWaveProvider == null)
            {
                return;
            }

            try
            {
                avatarWaveProvider.ClearBuffer();
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Error clearing avatar wave provider buffer");
            }
            finally
            {
                avatarWaveProvider = null;
            }

            Console.WriteLine("🧹 Avatar audio resources cleaned up");
        }

        /// <summary>
        ///     Stops capture and playback and releases the devices.
        /// </summary>
        public void Dispose()
        {
            StopRecording();
            StopPlayback();

            if (waveIn != null)
            {
                waveIn.DataAvailable -= OnAudioDataAvailable;
                waveIn.RecordingStopped -= OnRecordingStopped;
                waveIn.Dispose();
                waveIn = null;
            }

            waveOut?.Dispose();
            waveOut = null;
            waveProvider = null;
            avatarWaveProvider = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Sends each captured buffer on. Declared <c>async void</c> because it is an event handler; every
        ///     failure is therefore caught here rather than escaping to the capture thread.
        /// </summary>
        /// <param name="sender">The capture device.</param>
        /// <param name="e">The captured buffer.</param>
        private async void OnAudioDataAvailable(object? sender, WaveInEventArgs e)
        {
            Func<byte[], Task>? send = SendAudioAsync;
            if (!IsRecording || e.BytesRecorded <= 0 || send == null)
            {
                return;
            }

            try
            {
                var audioData = new byte[e.BytesRecorded];
                Array.Copy(e.Buffer, 0, audioData, 0, e.BytesRecorded);

                if (UseStereoEchoReference && echoReference != null && waveProvider != null)
                {
                    // Read the backlog without taking the waveProvider lock: this runs on the capture thread,
                    // and blocking here would stall the playback path (which locks waveProvider to add
                    // samples), producing choppy output. BufferedBytes is cheap and an approximation is fine.
                    audioData = echoReference.BuildStereoFrame(audioData, waveProvider.BufferedBytes);

                    // Periodic proof that channel 1 really carries playback audio, and how aligned it is.
                    if (Environment.TickCount - lastEcStatsTick >= EcStatsIntervalMs)
                    {
                        lastEcStatsTick = Environment.TickCount;
                        Console.WriteLine($"[EC ref] {echoReference.DescribeStats(sampleRate)}");
                    }
                }

                await send(audioData);
            }
            catch (Exception ex)
            {
                logger?.LogError("Error sending audio data: {Message}", ex.Message);
            }
        }

        /// <summary>
        ///     Logs why capture stopped.
        /// </summary>
        /// <param name="sender">The capture device.</param>
        /// <param name="e">The stop reason.</param>
        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            logger?.LogTrace("Recording stopped");
            if (e.Exception != null)
            {
                logger?.LogError("Recording error: {Message}", e.Exception.Message);
            }
        }

        #endregion
    }
}
