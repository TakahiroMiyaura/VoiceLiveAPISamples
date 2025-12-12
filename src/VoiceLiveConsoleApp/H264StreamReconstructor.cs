// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI;

/// <summary>
///     Reconstructs H.264 stream with periodic SPS/PPS injection for proper playback.
/// </summary>
public class H264StreamReconstructor
{
    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the H264StreamReconstructor class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public H264StreamReconstructor(ILogger logger)
    {
        this.logger = logger;
    }

    #endregion

    #region Public Methods

    /// <summary>
    ///     Processes an H.264 frame and returns reconstructed frame with periodic headers.
    /// </summary>
    /// <param name="frameData">Original H.264 frame data.</param>
    /// <returns>Reconstructed frame data with periodic SPS/PPS injection.</returns>
    public byte[] ProcessFrame(byte[] frameData)
    {
        if (frameData == null || frameData.Length == 0)
            return Array.Empty<byte>();

        try
        {
            frameCount++;

            // Parse NAL units from the frame
            var nalUnits = ParseNalUnits(frameData);

            // Cache SPS/PPS if found
            CacheSpsAndPps(nalUnits);

            // Check if we need to inject headers
            var needsHeaderInjection = ShouldInjectHeaders(nalUnits);

            if (needsHeaderInjection && cachedSPS != null && cachedPPS != null)
            {
                logger?.LogTrace("[H264StreamReconstructor] Frame {frame}: Injecting SPS/PPS headers", frameCount);
                return CreateFrameWithHeaders(frameData, nalUnits);
            }

            logger?.LogTrace("[H264StreamReconstructor] Frame {frame}: No header injection needed", frameCount);
            return frameData;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[H264StreamReconstructor] Error processing frame {frame}", frameCount);
            return frameData; // Return original on error
        }
    }

    #endregion

    #region Helper Classes

    /// <summary>
    ///     Represents an H.264 NAL unit.
    /// </summary>
    private class NalUnit
    {
        /// <summary>
        ///     NAL unit type.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        ///     Complete NAL unit data including start code.
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>
        ///     Offset in original frame.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        ///     Total size including start code.
        /// </summary>
        public int Size { get; set; }
    }

    #endregion

    #region Private Fields

    /// <summary>
    ///     Logger instance.
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    ///     Cached SPS NAL unit.
    /// </summary>
    private byte[] cachedSPS = Array.Empty<byte>();

    /// <summary>
    ///     Cached PPS NAL unit.
    /// </summary>
    private byte[] cachedPPS = Array.Empty<byte>();

    /// <summary>
    ///     Frame counter for periodic injection.
    /// </summary>
    private long frameCount;

    /// <summary>
    ///     Injection interval (every N frames).
    /// </summary>
    private const int InjectionInterval = 30; // Every 30 frames (~1 second at 30fps)

    #endregion

    #region Private Methods

    /// <summary>
    ///     Parses NAL units from H.264 frame data.
    /// </summary>
    /// <param name="data">H.264 frame data.</param>
    /// <returns>List of NAL unit information.</returns>
    private List<NalUnit> ParseNalUnits(byte[] data)
    {
        var nalUnits = new List<NalUnit>();

        for (var i = 0; i < data.Length - 4; i++)
        {
            // Look for NAL unit start codes
            var isStartCode = false;
            var startCodeLength = 0;

            if (data[i] == 0x00 && data[i + 1] == 0x00 && data[i + 2] == 0x00 && data[i + 3] == 0x01)
            {
                isStartCode = true;
                startCodeLength = 4;
            }
            else if (data[i] == 0x00 && data[i + 1] == 0x00 && data[i + 2] == 0x01)
            {
                isStartCode = true;
                startCodeLength = 3;
            }

            if (isStartCode)
            {
                var nalHeaderOffset = i + startCodeLength;
                if (nalHeaderOffset < data.Length)
                {
                    var nalType = data[nalHeaderOffset] & 0x1F;

                    // Find next start code to determine size
                    var nextStartCode = FindNextStartCode(data, nalHeaderOffset + 1);
                    var nalSize = nextStartCode > 0 ? nextStartCode - nalHeaderOffset : data.Length - nalHeaderOffset;

                    // Extract complete NAL unit including start code
                    var totalSize = startCodeLength + nalSize;
                    var nalData = new byte[totalSize];
                    Array.Copy(data, i, nalData, 0, totalSize);

                    nalUnits.Add(new NalUnit
                    {
                        Type = nalType,
                        Data = nalData,
                        Offset = i,
                        Size = totalSize
                    });

                    i = nalHeaderOffset; // Skip to avoid duplicate detection
                }
            }
        }

        return nalUnits;
    }

    /// <summary>
    ///     Finds the next NAL unit start code.
    /// </summary>
    /// <param name="data">H.264 data.</param>
    /// <param name="startOffset">Start search offset.</param>
    /// <returns>Offset of next start code, or -1 if not found.</returns>
    private int FindNextStartCode(byte[] data, int startOffset)
    {
        for (var i = startOffset; i < data.Length - 4; i++)
        {
            if ((data[i] == 0x00 && data[i + 1] == 0x00 && data[i + 2] == 0x00 && data[i + 3] == 0x01) ||
                (data[i] == 0x00 && data[i + 1] == 0x00 && data[i + 2] == 0x01))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    ///     Caches SPS and PPS NAL units when found.
    /// </summary>
    /// <param name="nalUnits">NAL units from current frame.</param>
    private void CacheSpsAndPps(List<NalUnit> nalUnits)
    {
        var sps = nalUnits.FirstOrDefault(n => n.Type == 7);
        var pps = nalUnits.FirstOrDefault(n => n.Type == 8);

        if (sps != null)
        {
            cachedSPS = sps.Data;
            logger?.LogInformation("[H264StreamReconstructor] SPS cached: {size} bytes", sps.Data.Length);
        }

        if (pps != null)
        {
            cachedPPS = pps.Data;
            logger?.LogInformation("[H264StreamReconstructor] PPS cached: {size} bytes", pps.Data.Length);
        }
    }

    /// <summary>
    ///     Determines if headers should be injected for the current frame.
    /// </summary>
    /// <param name="nalUnits">NAL units from current frame.</param>
    /// <returns>True if headers should be injected.</returns>
    private bool ShouldInjectHeaders(List<NalUnit> nalUnits)
    {
        // Always inject for IDR frames
        var hasIDR = nalUnits.Any(n => n.Type == 5);
        if (hasIDR)
            return true;

        // Inject periodically (every N frames)
        var isPeriodicFrame = frameCount % InjectionInterval == 0;
        if (isPeriodicFrame)
            return true;

        // Don't inject if frame already has SPS/PPS
        var hasSPS = nalUnits.Any(n => n.Type == 7);
        var hasPPS = nalUnits.Any(n => n.Type == 8);
        if (hasSPS || hasPPS)
            return false;

        return false;
    }

    /// <summary>
    ///     Creates a new frame with SPS/PPS headers prepended.
    /// </summary>
    /// <param name="originalFrame">Original frame data.</param>
    /// <param name="nalUnits">NAL units from original frame.</param>
    /// <returns>Frame with headers prepended.</returns>
    private byte[] CreateFrameWithHeaders(byte[] originalFrame, List<NalUnit> nalUnits)
    {
        // Calculate total size
        var totalSize = cachedSPS.Length + cachedPPS.Length + originalFrame.Length;
        var reconstructedFrame = new byte[totalSize];

        var offset = 0;

        // Add SPS
        Array.Copy(cachedSPS, 0, reconstructedFrame, offset, cachedSPS.Length);
        offset += cachedSPS.Length;

        // Add PPS
        Array.Copy(cachedPPS, 0, reconstructedFrame, offset, cachedPPS.Length);
        offset += cachedPPS.Length;

        // Add original frame
        Array.Copy(originalFrame, 0, reconstructedFrame, offset, originalFrame.Length);

        logger?.LogTrace("[H264StreamReconstructor] Reconstructed frame: {original} + {headers} = {total} bytes",
            originalFrame.Length, cachedSPS.Length + cachedPPS.Length, totalSize);

        return reconstructedFrame;
    }

    #endregion
}