// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Avatars.Streaming
{
    /// <summary>
    ///     Renders avatar video that is delivered over the session WebSocket control channel as
    ///     <c>response.video.delta</c> events, by piping the frames into an FFplay window.
    ///     The service delivers a <b>fragmented MP4 (fMP4)</b> byte stream — the first frame is the
    ///     initialization segment (<c>ftyp</c> + <c>moov</c>) and subsequent frames are media fragments
    ///     (<c>moof</c> + <c>mdat</c>), with H.264 video inside. Frames are written verbatim to FFplay's
    ///     MP4 demuxer (no Annex-B reconstruction).
    ///     Unlike <see cref="AvatarVideoStreamer" />, this path uses no WebRTC/SIPSorcery: frames arrive
    ///     on the same WebSocket as the rest of the session (avatar config <c>output_protocol=websocket</c>),
    ///     and the spoken audio flows through the normal <c>response.audio.delta</c> PCM path.
    /// </summary>
    public class WebSocketAvatarVideoStreamer : IDisposable
    {
        #region Private Fields

        private readonly ILogger logger;
        private readonly object writeLock = new();

        private Process? ffplayProcess;
        private Stream? ffplayInput;
        private long frameCount;
        private bool started;
        private bool disposed;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a value indicating whether the FFplay video window is currently active.
        /// </summary>
        public bool IsStreaming => started && ffplayProcess is { HasExited: false };

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WebSocketAvatarVideoStreamer" /> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public WebSocketAvatarVideoStreamer(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Starts the FFplay process that reads a fragmented MP4 (fMP4) stream from standard input.
        /// </summary>
        /// <returns><c>true</c> if FFplay started successfully; otherwise <c>false</c>.</returns>
        public bool Start()
        {
            if (started)
            {
                return true;
            }

            try
            {
                // Read a fragmented MP4 stream from stdin with low-latency flags; open a playback window.
                const string args = "-hide_banner -loglevel warning -f mp4 " +
                                    "-fflags nobuffer -flags low_delay -framedrop " +
                                    "-window_title \"Avatar (WebSocket)\" -i pipe:0";

                ffplayProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffplay",
                        Arguments = args,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        RedirectStandardInput = true,
                        RedirectStandardError = true
                    }
                };

                if (!ffplayProcess.Start())
                {
                    logger.LogError("[WebSocketAvatarVideoStreamer] Failed to start FFplay process");
                    return false;
                }

                ffplayInput = ffplayProcess.StandardInput.BaseStream;
                _ = Task.Run(DrainStandardErrorAsync);

                started = true;
                logger.LogInformation("[WebSocketAvatarVideoStreamer] FFplay started (ffplay {args})", args);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "[WebSocketAvatarVideoStreamer] Error starting FFplay. Ensure ffplay is available in PATH.");
                return false;
            }
        }

        /// <summary>
        ///     Writes one fragmented-MP4 frame (from a <c>response.video.delta</c> event) verbatim to FFplay's
        ///     MP4 demuxer. The first frame is the fMP4 initialization segment and subsequent frames are media
        ///     fragments; no Annex-B reconstruction is performed.
        /// </summary>
        /// <param name="frameData">The fMP4 frame bytes decoded from the event payload.</param>
        public void WriteFrame(byte[] frameData)
        {
            if (!started || frameData == null || frameData.Length == 0)
            {
                return;
            }

            try
            {
                // Guard the disposed/exited checks and the write together so Dispose cannot close the stream
                // mid-write (both take writeLock).
                lock (writeLock)
                {
                    if (disposed || ffplayInput == null || ffplayProcess is not { HasExited: false })
                    {
                        return;
                    }

                    frameCount++;

                    // Log the first few frames' leading bytes to confirm the container (fMP4: ftyp/moof).
                    if (frameCount <= 3)
                    {
                        logger.LogInformation(
                            "[WebSocketAvatarVideoStreamer] Frame #{n}: {in} bytes, head=[{head}]",
                            frameCount, frameData.Length, DescribeHead(frameData));
                    }

                    ffplayInput.Write(frameData, 0, frameData.Length);
                    ffplayInput.Flush();
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[WebSocketAvatarVideoStreamer] Error writing frame #{n}", frameCount);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Formats the first bytes of a frame as hex for diagnostic logging.
        /// </summary>
        /// <param name="data">The frame bytes.</param>
        /// <returns>A space-separated hex string of the leading bytes.</returns>
        private static string DescribeHead(byte[] data)
        {
            int count = Math.Min(8, data.Length);
            string[] parts = new string[count];
            for (int i = 0; i < count; i++)
            {
                parts[i] = data[i].ToString("X2");
            }

            return string.Join(" ", parts);
        }

        /// <summary>
        ///     Drains FFplay's standard error stream into the logger so playback issues are visible.
        /// </summary>
        private async Task DrainStandardErrorAsync()
        {
            try
            {
                while (ffplayProcess is { HasExited: false })
                {
                    string? line = await ffplayProcess.StandardError.ReadLineAsync();
                    if (!string.IsNullOrEmpty(line))
                    {
                        logger.LogTrace("[WebSocketAvatarVideoStreamer] FFplay: {line}", line);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogTrace(ex, "[WebSocketAvatarVideoStreamer] Stopped reading FFplay output");
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        ///     Releases resources and terminates the FFplay process.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            // Close the input under writeLock so an in-flight WriteFrame cannot write to a closed stream.
            lock (writeLock)
            {
                disposed = true;
                started = false;

                try
                {
                    ffplayInput?.Flush();
                    ffplayInput?.Close();
                }
                catch (Exception ex)
                {
                    logger.LogTrace(ex, "[WebSocketAvatarVideoStreamer] Error closing FFplay input");
                }

                ffplayInput = null;
            }

            try
            {
                if (ffplayProcess is { HasExited: false })
                {
                    ffplayProcess.Kill();
                }

                ffplayProcess?.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[WebSocketAvatarVideoStreamer] Error disposing FFplay process");
            }

            ffplayProcess = null;
        }

        #endregion
    }
}
