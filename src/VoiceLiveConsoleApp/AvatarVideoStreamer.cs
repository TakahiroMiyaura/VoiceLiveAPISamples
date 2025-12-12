// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars;
using Concentus;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using SIPSorceryMedia.Abstractions;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI;

/// <summary>
///     Avatar video streamer with RTP timestamp-based synchronization.
///     Uses timestamp information to properly synchronize audio and video playback.
/// </summary>
public class AvatarVideoStreamer : IDisposable
{
    #region Constructors

    /// <summary>
    ///     jj
    ///     Initializes a new instance of the AvatarVideoStreamer class.
    /// </summary>
    /// <param name="avatarClient">Avatar client instance.</param>
    /// <param name="logger">Logger instance.</param>
    public AvatarVideoStreamer(AvatarClient avatarClient, ILogger logger)
    {
        this.avatarClient = avatarClient ?? throw new ArgumentNullException(nameof(avatarClient));
        this.logger = logger;

        logger?.LogTrace("[AvatarVideoStreamer] Starting constructor initialization...");

        // Initialize H.264 processing components
        try
        {
            streamReconstructor = new H264StreamReconstructor(this.logger);
            logger?.LogTrace("[AvatarVideoStreamer] H.264 stream reconstructor initialized");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[AvatarVideoStreamer] Failed to initialize H.264 stream reconstructor");
            // Don't throw - continue without these components
        }

        // Initialize Opus decoder
        try
        {
            // OpusCodecFactory を使用して OpusDecoder を生成
            opusDecoder = OpusCodecFactory.CreateDecoder(48000, 2); // 48kHz stereo
            logger?.LogTrace("[AvatarVideoStreamer] Opus decoder initialized");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[AvatarVideoStreamer] Failed to initialize Opus decoder - audio will be disabled");
            // Don't throw - continue without audio decoding
            opusDecoder = null;
        }

        // Initialize WaveOut for audio playback (always initialize as fallback, even if using FFplay)
        try
        {
            const int avatarSampleRate = 48000;
            const int channels = 2;
            const int bitsPerSample = 16;

            waveOut = new WaveOutEvent
            {
                // AUDIO QUALITY FIX: Increased from 30ms to 100ms for stability
                // 100ms is still low-latency (imperceptible to humans) but prevents audio dropouts
                DesiredLatency = 100,

                // Increased from 2 to 3 buffers for better handling of network jitter
                NumberOfBuffers = 3
            };

            waveProvider = new BufferedWaveProvider(new WaveFormat(avatarSampleRate, bitsPerSample, channels))
            {
                // AUDIO QUALITY FIX: Increased from 0.25s to 1s buffer
                // Larger buffer handles network jitter from Azure Avatar API without dropouts
                BufferLength = avatarSampleRate * channels * 2, // 1 second buffer

                DiscardOnBufferOverflow = true,

                // AUDIO QUALITY FIX: Changed from false to true
                // ReadFully=true prevents reading from empty buffer (which causes static/noise)
                // Ensures smooth playback by waiting for sufficient data
                ReadFully = true
            };

            // Monitor playback stopped events
            waveOut.PlaybackStopped += (sender, e) =>
            {
                totalPlaybackStops++;
                var timeSinceLastFrame = lastAudioFrameReceivedTime != DateTime.MinValue
                    ? (DateTime.Now - lastAudioFrameReceivedTime).TotalMilliseconds
                    : -1;

                if (e.Exception != null)
                {
                    logger?.LogError(e.Exception,
                        "[AvatarVideoStreamer] ⚠️ WaveOut playback stopped with exception (stop #{count}, {ms:F0}ms since last frame)",
                        totalPlaybackStops, timeSinceLastFrame);
                }
                else
                {
                    var bufferedMs = waveProvider != null
                        ? waveProvider.BufferedBytes * 1000.0 /
                          (waveProvider.WaveFormat.SampleRate * waveProvider.WaveFormat.Channels * 2)
                        : 0;
                    logger?.LogWarning(
                        "[AvatarVideoStreamer] ⚠️ WaveOut playback stopped (stop #{count}, buffered: {ms:F0}ms, {time:F0}ms since last frame)",
                        totalPlaybackStops, bufferedMs, timeSinceLastFrame);

                    // Auto-restart if streaming is still active and we have buffered audio
                    if (isStreaming && waveProvider != null && waveProvider.BufferedBytes > 0)
                    {
                        try
                        {
                            logger?.LogTrace(
                                "[AvatarVideoStreamer] 🔄 Auto-restarting WaveOut playback ({ms:F0}ms buffered)",
                                bufferedMs);
                            waveOut?.Play();
                        }
                        catch (Exception restartEx)
                        {
                            logger?.LogError(restartEx, "[AvatarVideoStreamer] ❌ Failed to auto-restart WaveOut");
                        }
                    }
                    else
                    {
                        logger?.LogWarning(
                            "[AvatarVideoStreamer] ❌ Cannot restart: isStreaming={streaming}, bufferedBytes={bytes}",
                            isStreaming, waveProvider?.BufferedBytes ?? 0);
                    }
                }
            };

            waveOut.Init(waveProvider);
            logger?.LogTrace(
                "[AvatarVideoStreamer] WaveOut initialized: {rate}Hz, {channels}ch (available as fallback)",
                avatarSampleRate, channels);

            if (useFFplayAudio)
            {
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex,
                "[AvatarVideoStreamer] Failed to initialize WaveOut - audio playback will require FFplay");
            // Don't throw - continue with FFplay-only mode
            waveOut = null;
            waveProvider = null;
        }

        // Subscribe to avatar client events (always do this, even if WaveOut init failed)
        logger?.LogTrace("[AvatarVideoStreamer] Registering event handlers...");
        avatarClient.OnVideoFrameReceived += OnVideoFrame;
        logger?.LogTrace("[AvatarVideoStreamer] ✓ Video frame event handler registered");
        avatarClient.OnAudioFrameReceived += OnAudioFrame;
        logger?.LogTrace("[AvatarVideoStreamer] ✓ Audio frame event handler registered");

        logger?.LogTrace("[AvatarVideoStreamer] Constructor completed successfully. All components initialized.");
    }

    #endregion

    #region Private Fields

    /// <summary>
    ///     Avatar client for receiving frames.
    /// </summary>
    private readonly AvatarClient avatarClient;

    /// <summary>
    ///     Logger instance.
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    ///     H.264 stream reconstructor.
    /// </summary>
    private readonly H264StreamReconstructor? streamReconstructor;

    /// <summary>
    ///     Opus decoder for audio frames.
    /// </summary>
    private readonly IOpusDecoder? opusDecoder;

    /// <summary>
    ///     Wave output for audio playback.
    /// </summary>
    private readonly WaveOutEvent? waveOut;

    /// <summary>
    ///     Buffered wave provider for audio.
    /// </summary>
    private readonly BufferedWaveProvider? waveProvider;

    /// <summary>
    ///     Video frame queue (timestamp, frame data).
    /// </summary>
    private readonly ConcurrentQueue<(uint timestamp, byte[] data)> videoFrameQueue = new();

    /// <summary>
    ///     Audio frame queue (timestamp, decoded PCM data).
    /// </summary>
    private readonly ConcurrentQueue<(uint timestamp, byte[] pcmData, double dbLevel)> audioFrameQueue = new();

    /// <summary>
    ///     Session start time for timestamp-based synchronization.
    /// </summary>
    private DateTime? sessionStartTime;

    /// <summary>
    ///     Initial buffering duration before starting synchronized playback.
    ///     Reduced to 100ms to minimize audio delay.
    /// </summary>
    private readonly TimeSpan initialBufferingDuration = TimeSpan.FromMilliseconds(100);

    /// <summary>
    ///     Flag indicating initial buffering is complete.
    /// </summary>
    private bool initialBufferingComplete;

    /// <summary>
    ///     Enable timestamp-based synchronization (false = immediate playback like V3).
    ///     Set to false for debugging or if sync causes issues.
    /// </summary>
    private readonly bool enableTimestampSync = false; // Disabled: use immediate playback with synchronized start

    /// <summary>
    ///     Use FFplay for audio instead of WaveOut (eliminates WaveOut buffering).
    /// </summary>
    private readonly bool useFFplayAudio = true; // Enabled: use FFplay for low-latency audio

    /// <summary>
    ///     FFmpeg process for video file writing.
    /// </summary>
    private Process? ffmpegProcess;

    /// <summary>
    ///     FFplay process for real-time playback.
    /// </summary>
    private Process? ffplayRealtimeProcess;

    /// <summary>
    ///     Audio input stream to FFplay (for PCM audio).
    /// </summary>
    private Stream? ffplayAudioInputStream;

    /// <summary>
    ///     Video input stream to FFmpeg.
    /// </summary>
    private Stream? ffmpegVideoInputStream;

    /// <summary>
    ///     Flag indicating Named Pipe connection is established.
    /// </summary>
    private volatile bool audioPipeConnected;

    /// <summary>
    ///     Flag indicating WAV header has been written to Named Pipe.
    /// </summary>
    private volatile bool wavHeaderWritten;

    /// <summary>
    ///     Streaming active flag.
    /// </summary>
    private bool isStreaming;

    /// <summary>
    ///     Video frame counter.
    /// </summary>
    private long videoFrameCount;

    /// <summary>
    ///     Video frames written to FFmpeg counter (for diagnostics).
    /// </summary>
    private int videoFramesWrittenToFFmpeg;

    /// <summary>
    ///     Flag indicating first IDR frame (with SPS/PPS) has been received.
    /// </summary>
    private bool firstIdrFrameReceived;

    /// <summary>
    ///     Flag indicating first video frame has been written to FFmpeg (triggers FFplay startup).
    /// </summary>
    private bool firstVideoFrameWritten;

    /// <summary>
    ///     Audio frame counter.
    /// </summary>
    private long audioFrameCount;

    /// <summary>
    ///     First video timestamp (reference point).
    /// </summary>
    private uint? firstVideoTimestamp;

    /// <summary>
    ///     First audio timestamp (reference point).
    /// </summary>
    private uint? firstAudioTimestamp;

    /// <summary>
    ///     Synchronization thread.
    /// </summary>
    private Thread? syncThread;

    /// <summary>
    ///     Cancellation token source.
    /// </summary>
    private CancellationTokenSource? cancellationTokenSource;

    /// <summary>
    ///     Wave playback started flag.
    /// </summary>
    private bool wavePlaybackStarted;

    /// <summary>
    ///     Minimum buffered frames before starting playback.
    ///     AUDIO QUALITY FIX: Increased from 2 to 10 frames for stability.
    ///     10 frames ≈ 200ms provides smooth playback start without dropouts.
    ///     200ms latency is imperceptible in conversation (&lt;250ms feels natural).
    /// </summary>
    private const int MinBufferedFrames = 10;

    /// <summary>
    ///     Video clock rate (H.264 standard).
    /// </summary>
    private const double VideoClockRate = 90000.0;

    /// <summary>
    ///     Audio clock rate (Opus 48kHz).
    /// </summary>
    private const double AudioClockRate = 48000.0;

    /// <summary>
    ///     Last received audio frame timestamp for monitoring.
    /// </summary>
    private DateTime lastAudioFrameReceivedTime = DateTime.MinValue;

    /// <summary>
    ///     Previous audio frame received time for inter-frame timing analysis.
    /// </summary>
    private DateTime previousAudioFrameReceivedTime = DateTime.MinValue;

    /// <summary>
    ///     Total Opus decode errors.
    /// </summary>
    private long totalDecodeErrors;

    /// <summary>
    ///     Total playback stop events.
    /// </summary>
    private long totalPlaybackStops;

    /// <summary>
    ///     Skip first Opus frame (often metadata/header).
    /// </summary>
    private bool skipFirstOpusFrame = true;

    #endregion

    #region Public Methods

    /// <summary>
    ///     Starts streaming with timestamp-based synchronization.
    /// </summary>
    /// <returns>True if started successfully.</returns>
    public bool StartStreaming()
    {
        if (isStreaming)
            return true;

        try
        {
            cancellationTokenSource = new CancellationTokenSource();

            // Start FFmpeg process for video + audio (integrated)
            if (StartFFmpegProcess())
            {
                // NOTE: Audio is now handled by FFmpeg via Named Pipe
                // No separate FFplay audio process needed

                isStreaming = true;
                videoFrameCount = 0;
                audioFrameCount = 0;
                firstVideoTimestamp = null;
                firstAudioTimestamp = null;
                wavePlaybackStarted = false;
                sessionStartTime = null;
                initialBufferingComplete = false;
                audioPipeConnected = false;
                wavHeaderWritten = false;
                firstIdrFrameReceived = false; // Wait for first IDR keyframe with SPS/PPS
                firstVideoFrameWritten = false; // Delay FFplay startup until first frame is written

                // Start synchronization thread
                syncThread = new Thread(SynchronizationLoop)
                {
                    IsBackground = true,
                    Name = "V6-Sync-Thread"
                };
                syncThread.Start();

                logger?.LogTrace("[AvatarVideoStreamer] Synchronized A/V streaming started (V6)");
                logger?.LogTrace("[AvatarVideoStreamer] Video clock: {rate}Hz, Audio clock: {rate2}Hz",
                    VideoClockRate, AudioClockRate);

                return true;
            }

            logger?.LogError("[AvatarVideoStreamer] Failed to start FFmpeg process");
            return false;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[AvatarVideoStreamer] Error starting streaming");
            return false;
        }
    }

    /// <summary>
    ///     Stops streaming.
    /// </summary>
    public void StopStreaming()
    {
        if (!isStreaming)
            return;

        try
        {
            isStreaming = false;

            logger?.LogTrace("[AvatarVideoStreamer] Stopping streaming. Video frames: {video}, Audio frames: {audio}",
                videoFrameCount, audioFrameCount);

            // Cancel synchronization thread
            cancellationTokenSource?.Cancel();
            syncThread?.Join(TimeSpan.FromSeconds(5));

            // Stop audio playback
            if (wavePlaybackStarted)
            {
                waveOut?.Stop();
            }

            waveProvider?.ClearBuffer();

            // Close FFmpeg input streams gracefully
            // Step 1: Flush and wait for FFmpeg to read buffered data
            try
            {
                if (ffmpegVideoInputStream != null)
                {
                    logger?.LogTrace(
                        "[AvatarVideoStreamer] Flushing video stream and waiting for FFmpeg to read data...");

                    // Flush to ensure all data is sent
                    ffmpegVideoInputStream.Flush();

                    // CRITICAL: Wait for FFmpeg to read the data before closing the stream
                    // Without this delay, stream closes before FFmpeg reads → frame=0
                    Thread.Sleep(500); // 500ms should be enough for FFmpeg to read buffered data

                    logger?.LogTrace("[AvatarVideoStreamer] Closing video input stream (sending EOF to FFmpeg)");
                    ffmpegVideoInputStream.Close();
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "[AvatarVideoStreamer] Error closing video stream");
            }

            // Step 3: Wait for FFmpeg to finish processing and exit gracefully
            if (ffmpegProcess != null && !ffmpegProcess.HasExited)
            {
                logger?.LogTrace("[AvatarVideoStreamer] Waiting for FFmpeg to finish processing...");

                // Wait up to 5 seconds for FFmpeg to exit gracefully
                if (!ffmpegProcess.WaitForExit(5000))
                {
                    logger?.LogWarning("[AvatarVideoStreamer] FFmpeg did not exit gracefully, forcing termination");
                    try
                    {
                        ffmpegProcess.Kill();
                    }
                    catch (Exception killEx)
                    {
                        logger?.LogWarning(killEx, "[AvatarVideoStreamer] Error killing FFmpeg process");
                    }
                }
                else
                {
                    logger?.LogTrace("[AvatarVideoStreamer] FFmpeg exited gracefully");
                }
            }

            // Step 4: Clean up FFplay real-time process
            if (ffplayRealtimeProcess != null && !ffplayRealtimeProcess.HasExited)
            {
                try
                {
                    logger?.LogTrace("[AvatarVideoStreamer] Closing FFplay video window");
                    ffplayRealtimeProcess.Kill();
                    ffplayRealtimeProcess.Dispose();
                    ffplayRealtimeProcess = null;
                }
                catch (Exception ffplayEx)
                {
                    logger?.LogWarning(ffplayEx, "[AvatarVideoStreamer] Error closing FFplay");
                }
            }

            // Stop real-time FFplay (video)
            if (ffplayRealtimeProcess != null && !ffplayRealtimeProcess.HasExited)
            {
                try
                {
                    ffplayRealtimeProcess.Kill();
                    ffplayRealtimeProcess.WaitForExit(2000);
                    logger?.LogTrace("[AvatarVideoStreamer] Real-time FFplay (video) stopped");
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "[AvatarVideoStreamer] Error stopping FFplay video");
                }
            }

            // Close FFplay audio input stream
            if (ffplayAudioInputStream != null)
            {
                try
                {
                    ffplayAudioInputStream.Flush();
                    ffplayAudioInputStream.Close();
                    ffplayAudioInputStream = null;
                    logger?.LogTrace("[AvatarVideoStreamer] FFplay audio input stream closed");
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "[AvatarVideoStreamer] Error closing FFplay audio input stream");
                }
            }

            // Wait for FFmpeg to finish
            if (ffmpegProcess?.HasExited == false)
            {
                ffmpegProcess.WaitForExit(5000);
                if (!ffmpegProcess.HasExited)
                {
                    logger?.LogWarning("[AvatarVideoStreamer] FFmpeg did not exit cleanly, killing process");
                    ffmpegProcess.Kill();
                }
            }

            // Clear queues
            while (videoFrameQueue.TryDequeue(out _))
            {
            }

            while (audioFrameQueue.TryDequeue(out _))
            {
            }

            logger?.LogTrace("[AvatarVideoStreamer] Streaming stopped successfully");
            logger?.LogTrace("[AvatarVideoStreamer] Video queue remaining: {count}", videoFrameQueue.Count);
            logger?.LogTrace("[AvatarVideoStreamer] Audio queue remaining: {count}", audioFrameQueue.Count);
            logger?.LogTrace(
                "[AvatarVideoStreamer] 📊 Statistics: Audio frames: {audio}, Decode errors: {errors}, Playback stops: {stops}",
                audioFrameCount, totalDecodeErrors, totalPlaybackStops);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[AvatarVideoStreamer] Error stopping streaming");
        }
    }

    /// <summary>
    ///     Disposes the AvatarVideoStreamer.
    /// </summary>
    public void Dispose()
    {
        StopStreaming();

        // Dispose audio resources
        waveOut?.Dispose();
        waveProvider?.ClearBuffer();

        // Dispose FFmpeg process
        ffmpegProcess?.Dispose();

        if (ffplayRealtimeProcess != null && !ffplayRealtimeProcess.HasExited)
        {
            try
            {
                ffplayRealtimeProcess.Kill();
                ffplayRealtimeProcess.Dispose();
            }
            catch
            {
                /* Ignore errors on cleanup */
            }
        }

        // Dispose other resources
        cancellationTokenSource?.Dispose();

        // Unsubscribe from events
        if (avatarClient != null)
        {
            avatarClient.OnVideoFrameReceived -= OnVideoFrame;
            avatarClient.OnAudioFrameReceived -= OnAudioFrame;
        }

        GC.SuppressFinalize(this);
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///     Starts FFmpeg process for video file writing and UDP streaming.
    /// </summary>
    /// <returns>True if started successfully.</returns>
    private bool StartFFmpegProcess()
    {
        try
        {
            // RTP streaming for real-time playback only (no file output)
            const int udpPort = 8888;

            var ffmpegArgs = $"-re -f h264 -r 25 -probesize 1024 -analyzeduration 1000000 " +
                             $"-fflags +genpts+nobuffer -flags low_delay " +
                             $"-i pipe:0 " +
                             // RTP streaming (video only) - designed for real-time
                             $"-c:v copy -r 25 -f rtp -payload_type 96 rtp://127.0.0.1:{udpPort}";

            ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ffmpegArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            if (ffmpegProcess.Start())
            {
                ffmpegVideoInputStream = ffmpegProcess.StandardInput.BaseStream;

                // Log FFmpeg errors in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var lineCount = 0;
                        while (!ffmpegProcess.HasExited)
                        {
                            var error = await ffmpegProcess.StandardError.ReadLineAsync();
                            if (!string.IsNullOrEmpty(error))
                            {
                                lineCount++;
                                // Log ALL lines for debugging (temporary - increase to 100)
                                if (lineCount <= 100)
                                {
                                    logger?.LogTrace("[AvatarVideoStreamer] FFmpeg #{count}: {error}", lineCount,
                                        error);
                                }
                                // Then only log important messages
                                else if (error.Contains("error") || error.Contains("Error") ||
                                         error.Contains("warning") || error.Contains("Warning") ||
                                         error.Contains("failed") || error.Contains("Failed") ||
                                         error.Contains("Stream mapping") || error.Contains("Output") ||
                                         error.Contains("frame=") || error.Contains("size="))
                                {
                                    logger?.LogTrace("[AvatarVideoStreamer] FFmpeg: {error}", error);
                                }
                            }
                        }

                        logger?.LogTrace("[AvatarVideoStreamer] FFmpeg process exited. Total lines: {count}",
                            lineCount);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "[AvatarVideoStreamer] Error reading FFmpeg output");
                    }
                });

                logger?.LogTrace(
                    "[AvatarVideoStreamer] FFmpeg process started successfully (RTP streaming mode)");
                logger?.LogTrace("[AvatarVideoStreamer] FFmpeg command: ffmpeg {args}", ffmpegArgs);

                return true;
            }

            logger?.LogError("[AvatarVideoStreamer] Failed to start FFmpeg process");
            return false;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[AvatarVideoStreamer] Error starting FFmpeg process");
            return false;
        }
    }

    /// <summary>
    ///     Creates an SDP (Session Description Protocol) file for FFplay to understand the RTP stream format.
    /// </summary>
    /// <param name="rtpPort">RTP port number.</param>
    /// <param name="sdpFilePath">Path to save the SDP file.</param>
    private void CreateSdpFile(int rtpPort, string sdpFilePath)
    {
        // SDP file helps FFplay understand the stream format before connecting
        // This eliminates the need for probing and reduces startup latency
        var sdpContent = $@"v=0
o=- 0 0 IN IP4 127.0.0.1
s=Avatar Video Stream
c=IN IP4 127.0.0.1
t=0 0
m=video {rtpPort} RTP/AVP 96
a=rtpmap:96 H264/90000
a=fmtp:96 packetization-mode=1";

        File.WriteAllText(sdpFilePath, sdpContent);
        logger?.LogTrace("[AvatarVideoStreamer] Created SDP file: {path}", sdpFilePath);
    }

    /// <summary>
    ///     Starts FFplay for real-time playback using multiple streaming protocols.
    /// </summary>
    /// <param name="port">Port number for streaming.</param>
    /// <param name="protocol">Protocol to use: "rtp", "udp", or "tcp".</param>
    private void StartRealtimeFFplay(int port, string protocol = "rtp")
    {
        try
        {
            ProcessStartInfo startInfo;

            if (protocol == "rtp")
            {
                // RTP protocol with SDP file (most robust for real-time streaming)
                // SDP file eliminates probing and provides format information upfront
                var sdpFilePath = Path.Combine(Directory.GetCurrentDirectory(), "avatar_stream.sdp");
                CreateSdpFile(port, sdpFilePath);

                startInfo = new ProcessStartInfo
                {
                    FileName = "ffplay",
                    Arguments =
                        $"-protocol_whitelist file,rtp,udp -fflags nobuffer -flags low_delay -framedrop -autoexit \"{sdpFilePath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                logger?.LogTrace("[AvatarVideoStreamer] Starting FFplay with RTP (SDP file: {sdp})", sdpFilePath);
            }
            else if (protocol == "tcp")
            {
                // TCP streaming (reliable but higher latency)
                startInfo = new ProcessStartInfo
                {
                    FileName = "ffplay",
                    Arguments =
                        $"-f mpegts -fflags nobuffer -flags low_delay -framedrop -autoexit tcp://127.0.0.1:{port}?listen",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                logger?.LogTrace("[AvatarVideoStreamer] Starting FFplay with TCP on port {port}", port);
            }
            else // UDP (original approach)
            {
                // Raw UDP streaming (connectionless, may have packet loss)
                startInfo = new ProcessStartInfo
                {
                    FileName = "ffplay",
                    Arguments =
                        $"-f mpegts -probesize 10M -analyzeduration 5M -fflags nobuffer -flags low_delay -framedrop -infbuf -autoexit udp://127.0.0.1:{port}",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                logger?.LogTrace("[AvatarVideoStreamer] Starting FFplay with UDP on port {port}", port);
            }

            ffplayRealtimeProcess = Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "[AvatarVideoStreamer] Failed to start real-time FFplay with {protocol}",
                protocol);
        }
    }

    /// <summary>
    ///     Synchronization loop that processes queued frames based on timestamps.
    /// </summary>
    private void SynchronizationLoop()
    {
        logger?.LogTrace("[AvatarVideoStreamer] Synchronization thread started");

        var lastStatusLog = DateTime.Now;

        try
        {
            while (isStreaming && cancellationTokenSource is { Token.IsCancellationRequested: false })
            {
                // Log queue status every 2 seconds for debugging
                if ((DateTime.Now - lastStatusLog).TotalSeconds >= 2)
                {
                    var bufferedMs = waveProvider != null
                        ? waveProvider.BufferedBytes * 1000.0 /
                          (waveProvider.WaveFormat.SampleRate * waveProvider.WaveFormat.Channels * 2)
                        : 0;
                    var playbackState = waveOut?.PlaybackState.ToString() ?? "null";
                    var timeSinceLastFrame = lastAudioFrameReceivedTime != DateTime.MinValue
                        ? (DateTime.Now - lastAudioFrameReceivedTime).TotalMilliseconds
                        : -1;

                    var syncStatus = initialBufferingComplete ? "Synced" : "Buffering";
                    logger?.LogTrace(
                        "[AvatarVideoStreamer] Status: {sync}, VideoQ={video}, AudioQ={audioQ}, WaveBuffer={ms:F0}ms, State={state}, Frames={frames}, Errors={errors}, Stops={stops}, LastAgo={ago:F0}ms",
                        syncStatus, videoFrameQueue.Count, audioFrameQueue.Count, bufferedMs, playbackState,
                        audioFrameCount, totalDecodeErrors, totalPlaybackStops, timeSinceLastFrame);
                    lastStatusLog = DateTime.Now;
                }

                // Process queued video and audio frames based on RTP timestamps
                ProcessSynchronizedFrames();

                // Sleep briefly to avoid busy waiting
                Thread.Sleep(10);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[AvatarVideoStreamer] Error in synchronization loop");
        }

        logger?.LogTrace("[AvatarVideoStreamer] Synchronization thread stopped");
    }

    /// <summary>
    ///     Processes synchronized frames from queues based on RTP timestamps.
    /// </summary>
    private void ProcessSynchronizedFrames()
    {
        // Wait for session to start and initial buffering
        if (!sessionStartTime.HasValue)
            return;

        var sessionElapsed = DateTime.Now - sessionStartTime.Value;

        // Complete initial buffering period
        if (!initialBufferingComplete)
        {
            if (sessionElapsed >= initialBufferingDuration)
            {
                initialBufferingComplete = true;
                logger?.LogTrace(
                    "[AvatarVideoStreamer] ⏰ Initial buffering complete ({ms:F0}ms), starting synchronized playback",
                    initialBufferingDuration.TotalMilliseconds);
                logger?.LogTrace("[AvatarVideoStreamer] Video queue: {video} frames, Audio queue: {audio} frames",
                    videoFrameQueue.Count, audioFrameQueue.Count);

                // Start WaveOut for audio playback
                if (!wavePlaybackStarted && waveOut != null)
                {
                    try
                    {
                        waveOut.Play();
                        wavePlaybackStarted = true;
                        logger?.LogTrace("[AvatarVideoStreamer] WaveOut playback started");
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "[AvatarVideoStreamer] Failed to start WaveOut");
                    }
                }
            }
            else
            {
                // Still buffering
                return;
            }
        }

        // Calculate current playback time (in seconds since buffering completed)
        var currentPlaybackTime = (sessionElapsed - initialBufferingDuration).TotalSeconds;

        // Add tolerance for timestamp jitter (200ms ahead is acceptable)
        const double TimestampTolerance = 0.2; // 200ms tolerance

        // Process video frames whose timestamp has arrived
        var videoFramesProcessed = 0;
        while (videoFrameQueue.TryPeek(out var videoFrame) && firstVideoTimestamp.HasValue)
        {
            var videoRelativeTime = (videoFrame.timestamp - firstVideoTimestamp.Value) / VideoClockRate;

            // Process frame if its time has arrived or is within tolerance
            if (videoRelativeTime <= currentPlaybackTime + TimestampTolerance)
            {
                // Time to process this video frame
                videoFrameQueue.TryDequeue(out videoFrame);
                ProcessVideoFrame(videoFrame.data);
                videoFramesProcessed++;
            }
            else
            {
                // Future frame beyond tolerance, wait
                break;
            }
        }

        // Process audio frames whose timestamp has arrived
        var audioFramesProcessed = 0;
        while (audioFrameQueue.TryPeek(out var audioFrame) && firstAudioTimestamp.HasValue)
        {
            var audioRelativeTime = (audioFrame.timestamp - firstAudioTimestamp.Value) / AudioClockRate;

            // Process frame if its time has arrived or is within tolerance
            if (audioRelativeTime <= currentPlaybackTime + TimestampTolerance)
            {
                // Time to process this audio frame
                audioFrameQueue.TryDequeue(out audioFrame);
                ProcessAudioFrame(audioFrame.pcmData);
                audioFramesProcessed++;
            }
            else
            {
                // Future frame beyond tolerance, wait
                break;
            }
        }

        // Log processing activity periodically (more frequent for debugging)
        if ((videoFramesProcessed > 0 || audioFramesProcessed > 0) && audioFrameCount % 10 == 0)
        {
            logger?.LogTrace(
                "[AvatarVideoStreamer] ⏱️ Sync @ {time:F3}s: Processed {video}v/{audio}a frames, Queued: {vq}v/{aq}a",
                currentPlaybackTime, videoFramesProcessed, audioFramesProcessed,
                videoFrameQueue.Count, audioFrameQueue.Count);
        }

        // Warn if queues are growing without processing
        if (audioFrameQueue.Count > 50 && audioFramesProcessed == 0 && initialBufferingComplete)
        {
            logger?.LogWarning(
                "[AvatarVideoStreamer] ⚠️ Audio queue growing ({count} frames) but no frames processed! Current time: {time:F3}s, First timestamp: {ts}",
                audioFrameQueue.Count, currentPlaybackTime, firstAudioTimestamp);

            // Log next frame info for debugging
            if (audioFrameQueue.TryPeek(out var nextFrame))
            {
                if (firstAudioTimestamp != null)
                {
                    var nextRelativeTime = (nextFrame.timestamp - firstAudioTimestamp.Value) / AudioClockRate;
                    logger?.LogWarning(
                        "[AvatarVideoStreamer] Next audio frame time: {time:F3}s (current: {current:F3}s, diff: {diff:F3}s)",
                        nextRelativeTime, currentPlaybackTime, nextRelativeTime - currentPlaybackTime);
                }
            }
        }
    }

    /// <summary>
    ///     Checks if frame contains a specific NAL unit type.
    /// </summary>
    private static bool HasNALUnit(byte[] data, byte nalType)
    {
        for (var i = 0; i < data.Length - 4; i++)
        {
            // Look for NAL start code: 0x00 0x00 0x00 0x01 or 0x00 0x00 0x01
            if (data[i] == 0x00 && data[i + 1] == 0x00)
            {
                var nalStart = -1;
                if (data[i + 2] == 0x00 && data[i + 3] == 0x01)
                    nalStart = i + 4;
                else if (data[i + 2] == 0x01)
                    nalStart = i + 3;

                if (nalStart != -1 && nalStart < data.Length)
                {
                    var nalUnitType = data[nalStart] & 0x1F;
                    if (nalUnitType == nalType)
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Processes a single video frame (writes to FFmpeg).
    /// </summary>
    private void ProcessVideoFrame(byte[] frameData)
    {
        try
        {
            if (ffmpegVideoInputStream == null)
            {
                if (videoFramesWrittenToFFmpeg < 3)
                {
                    logger?.LogError(
                        "[AvatarVideoStreamer] Cannot write video frame - ffmpegVideoInputStream is NULL");
                }

                return;
            }

            if (!ffmpegVideoInputStream.CanWrite)
            {
                if (videoFramesWrittenToFFmpeg < 3)
                {
                    logger?.LogError("[AvatarVideoStreamer] Cannot write video frame - stream is not writable");
                }

                return;
            }

            // Check for SPS/PPS/IDR NAL units (required for FFmpeg to initialize H.264 decoder)
            var hasSPS = HasNALUnit(frameData, 7); // SPS (0x67 & 0x1F = 7)
            var hasPPS = HasNALUnit(frameData, 8); // PPS (0x68 & 0x1F = 8)
            var hasIDR = HasNALUnit(frameData, 5); // IDR frame (0x65 & 0x1F = 5)

            // CRITICAL: Wait for first IDR frame with SPS/PPS before sending to FFmpeg
            // FFmpeg cannot decode H.264 without these initialization parameters
            if (!firstIdrFrameReceived)
            {
                if (hasSPS && hasPPS && hasIDR)
                {
                    firstIdrFrameReceived = true;
                    logger?.LogTrace(
                        "[AvatarVideoStreamer] ✅ First IDR frame with SPS/PPS received - starting video encoding");
                }
                else
                {
                    // Skip this frame - waiting for proper IDR keyframe
                    if (videoFramesWrittenToFFmpeg == 0)
                    {
                        logger?.LogWarning(
                            "[AvatarVideoStreamer] ⏭️ Skipping non-IDR frame (waiting for SPS/PPS), SPS={sps}, PPS={pps}, IDR={idr}",
                            hasSPS, hasPPS, hasIDR);
                    }

                    return; // Don't write to FFmpeg yet
                }
            }

            // Write frame to FFmpeg
            ffmpegVideoInputStream.Write(frameData, 0, frameData.Length);
            videoFramesWrittenToFFmpeg++;

            // CRITICAL: Flush every frame for low-latency RTP streaming
            // Without this, FFmpeg buffers data and doesn't output in real-time
            ffmpegVideoInputStream.Flush();

            // NEW: Start FFplay after first frame is written and flushed
            // This ensures FFmpeg is already sending RTP data when FFplay connects
            if (!firstVideoFrameWritten && videoFramesWrittenToFFmpeg == 1)
            {
                firstVideoFrameWritten = true;
                logger?.LogTrace("[AvatarVideoStreamer] First video frame written and flushed - starting FFplay now");

                // Small delay to ensure FFmpeg starts RTP transmission
                Thread.Sleep(100);

                // Try RTP first (most robust for real-time), fallback options: "udp" or "tcp"
                StartRealtimeFFplay(8888);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[AvatarVideoStreamer] Error writing video frame #{count}",
                videoFramesWrittenToFFmpeg);
        }
    }

    /// <summary>
    ///     Processes a single audio frame (adds to playback buffer).
    /// </summary>
    private void ProcessAudioFrame(byte[] pcmData)
    {
        try
        {
            waveProvider?.AddSamples(pcmData, 0, pcmData.Length);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[AvatarVideoStreamer] Error adding audio to playback buffer");
        }
    }

    /// <summary>
    ///     Handles video frame reception with timestamp.
    /// </summary>
    private void OnVideoFrame(IPEndPoint remote, uint ssrc, byte[] frame, VideoFormat fmt, uint timestamp)
    {
        if (!isStreaming || frame == null || frame.Length == 0)
            return;

        try
        {
            videoFrameCount++;

            // Set first timestamp as reference
            if (!firstVideoTimestamp.HasValue)
            {
                firstVideoTimestamp = timestamp;
                logger?.LogTrace("[AvatarVideoStreamer] First video timestamp: {ts} (reference point)", timestamp);
            }

            // Reconstruct frame with SPS/PPS headers
            var reconstructedFrame = streamReconstructor?.ProcessFrame(frame) ?? [];

            // Add to queue with timestamp
            videoFrameQueue.Enqueue((timestamp, reconstructedFrame));

            // Set session start time on first frame (video or audio)
            if (!sessionStartTime.HasValue)
            {
                sessionStartTime = DateTime.Now;
                logger?.LogTrace("[AvatarVideoStreamer] ⏰ Session start time set (first video frame received)");
            }

            if (videoFrameCount % 30 == 1)
            {
                var relativeTime = (timestamp - firstVideoTimestamp.Value) / VideoClockRate;
                logger?.LogTrace(
                    "[AvatarVideoStreamer] Video frame #{count} queued, ts={ts}, relative={rel:F3}s, queue={queue}",
                    videoFrameCount, timestamp, relativeTime, videoFrameQueue.Count);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[AvatarVideoStreamer] Error processing video frame");
        }
    }

    /// <summary>
    ///     Handles audio frame reception with timestamp.
    /// </summary>
    private void OnAudioFrame(byte[] audioData, uint timestamp)
    {
        if (!isStreaming || audioData == null || audioData.Length == 0)
        {
            return;
        }

        try
        {
            audioFrameCount++;
            var currentFrameTime = DateTime.Now;

            // Calculate inter-frame timing
            var timeSinceLastFrame = previousAudioFrameReceivedTime != DateTime.MinValue
                ? (currentFrameTime - previousAudioFrameReceivedTime).TotalMilliseconds
                : 0;

            previousAudioFrameReceivedTime = currentFrameTime;
            lastAudioFrameReceivedTime = currentFrameTime;

            // Minimal logging only for first frame
            if (audioFrameCount == 1)
            {
                logger?.LogTrace("[AvatarVideoStreamer] First audio frame received: {bytes} bytes, timestamp={ts}",
                    audioData.Length, timestamp);
            }

            // Set first timestamp as reference
            if (!firstAudioTimestamp.HasValue)
            {
                firstAudioTimestamp = timestamp;
                logger?.LogTrace("[AvatarVideoStreamer] First audio timestamp: {ts} (reference point)", timestamp);
            }

            // Skip first Opus frame if it's likely metadata/header (63 bytes is suspicious)
            if (skipFirstOpusFrame && audioData.Length == 63)
            {
                logger?.LogTrace(
                    "[AvatarVideoStreamer] Skipping first Opus frame (likely header/metadata): {bytes} bytes",
                    audioData.Length);
                skipFirstOpusFrame = false;
                return; // Skip this frame
            }

            skipFirstOpusFrame = false; // Don't skip subsequent frames

            // Decode Opus to PCM and play immediately (like V3)
            // Note: waveProvider check removed - FFplay mode doesn't use waveProvider
            if (opusDecoder != null)
            {
                try
                {
                    // Opus frame can be 2.5ms to 60ms (120 to 2880 samples at 48kHz)
                    // Use maximum size buffer to handle all frame sizes
                    const int maxFrameSize = 5760; // 60ms at 48kHz stereo (2880 * 2)
                    var pcmBuffer = new short[maxFrameSize];

                    // 修正後:
                    var decodedSamples = opusDecoder.Decode(audioData, pcmBuffer, maxFrameSize / 2);

                    if (decodedSamples > 0)
                    {
                        // Convert short[] to byte[]
                        var pcmBytes = new byte[decodedSamples * 2 * 2]; // stereo, 16-bit
                        Buffer.BlockCopy(pcmBuffer, 0, pcmBytes, 0, pcmBytes.Length);

                        // Calculate audio level (RMS) for silence detection
                        double sumSquares = 0;
                        for (var i = 0; i < decodedSamples * 2; i++)
                        {
                            sumSquares += pcmBuffer[i] * pcmBuffer[i];
                        }

                        var rms = Math.Sqrt(sumSquares / (decodedSamples * 2));
                        var dbLevel = 20 * Math.Log10(rms / 32768.0); // Convert to dB
                        var isSilent = dbLevel < -60;

                        if (enableTimestampSync)
                        {
                            // Queue PCM data for timestamp-based synchronized playback
                            audioFrameQueue.Enqueue((timestamp, pcmBytes, dbLevel));

                            // Set session start time on first frame (video or audio)
                            if (!sessionStartTime.HasValue)
                            {
                                sessionStartTime = DateTime.Now;
                                logger?.LogTrace(
                                    "[AvatarVideoStreamer] ⏰ Session start time set (first audio frame received)");
                            }

                            // Log audio progress with level
                            if (audioFrameCount % 50 == 1 || audioFrameCount <= 10)
                            {
                                var relativeTime = (timestamp - firstAudioTimestamp.Value) / AudioClockRate;
                                logger?.LogTrace(
                                    "[AvatarVideoStreamer] Audio frame #{count} queued: {samples} samples → {bytes} bytes PCM, queue: {queue}, Level: {db:F1}dB",
                                    audioFrameCount, decodedSamples, pcmBytes.Length, audioFrameQueue.Count, dbLevel);
                            }
                        }
                        else
                        {
                            // Send PCM audio to FFmpeg via Named Pipe (synchronized A/V)
                            // Wait for pipe connection before sending data
                            if (audioPipeConnected && wavHeaderWritten)
                            {
                                try
                                {
                                    // Minimal logging
                                    if (audioFrameCount == 1)
                                    {
                                        logger?.LogTrace(
                                            "[AvatarVideoStreamer] First audio frame sent to FFmpeg pipe: {samples} samples → {bytes} bytes PCM",
                                            decodedSamples, pcmBytes.Length);
                                    }
                                }
                                catch (Exception pipeEx)
                                {
                                    logger?.LogWarning(pipeEx,
                                        "[AvatarVideoStreamer] Failed to send audio to Named Pipe");
                                }
                            }
                            else if (audioFrameCount == 1 && !audioPipeConnected)
                            {
                                // Log once if pipe not connected
                                logger?.LogWarning(
                                    "[AvatarVideoStreamer] Audio pipe not ready, connected={conn}, headerWritten={header}",
                                    audioPipeConnected, wavHeaderWritten);
                            }
                            else if (waveProvider != null)
                            {
                                // Fallback to WaveOut playback
                                // Check buffer level before adding samples
                                var bufferedMs = waveProvider.BufferedBytes * 1000.0 /
                                                 (waveProvider.WaveFormat.SampleRate *
                                                  waveProvider.WaveFormat.Channels * 2);

                                waveProvider.AddSamples(pcmBytes, 0, pcmBytes.Length);

                                // Start WaveOut after initial buffering
                                if (!wavePlaybackStarted && audioFrameCount >= MinBufferedFrames)
                                {
                                    try
                                    {
                                        waveOut?.Play();
                                        wavePlaybackStarted = true;
                                        bufferedMs = waveProvider.BufferedBytes * 1000.0 /
                                                     (waveProvider.WaveFormat.SampleRate *
                                                      waveProvider.WaveFormat.Channels * 2);
                                        logger?.LogTrace(
                                            "[AvatarVideoStreamer] WaveOut started after {frames} frames with {ms:F0}ms buffered audio",
                                            audioFrameCount, bufferedMs);
                                    }
                                    catch (Exception startEx)
                                    {
                                        logger?.LogWarning(startEx,
                                            "[AvatarVideoStreamer] Failed to start WaveOut after buffering");
                                    }
                                }
                            }
                        }

                        // Warn if audio is too quiet (likely silence) - applies to both modes
                        if (dbLevel < -60 && audioFrameCount % 50 == 1)
                        {
                            logger?.LogWarning(
                                "[AvatarVideoStreamer] ⚠️ Audio frame #{count} is very quiet ({db:F1}dB) - possible silence",
                                audioFrameCount, dbLevel);
                        }
                    }
                    else
                    {
                        // Opus decode succeeded but returned 0 or negative samples
                        logger?.LogWarning(
                            "[AvatarVideoStreamer] Opus decode returned {samples} samples for frame #{count} (input: {bytes} bytes)",
                            decodedSamples, audioFrameCount, audioData.Length);
                    }
                }
                catch (Exception decodeEx)
                {
                    totalDecodeErrors++;
                    // Log all decode errors for debugging audio cutoff issues
                    logger?.LogWarning(decodeEx,
                        "[AvatarVideoStreamer] ❌ Error decoding Opus frame #{count} (input: {bytes} bytes, total errors: {errors}): {message}",
                        audioFrameCount, audioData.Length, totalDecodeErrors, decodeEx.Message);
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[AvatarVideoStreamer] Error processing audio frame");
        }
    }

    #endregion
}