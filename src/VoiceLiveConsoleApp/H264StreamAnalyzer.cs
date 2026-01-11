// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System.Text;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{
    /// <summary>
    ///     Analyzes H.264 stream structure and NAL units.
    /// </summary>
    public class H264StreamAnalyzer
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the H264StreamAnalyzer class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public H264StreamAnalyzer(ILogger logger)
        {
            this.logger = logger;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string analysisPath = $"h264_analysis_{timestamp}.txt";

            analysisFile = new FileStream(analysisPath, FileMode.Create, FileAccess.Write);

            logger?.LogTrace("[H264StreamAnalyzer] Analysis file created: {path}", analysisPath);
        }

        #endregion

        #region Private Fields

        /// <summary>
        ///     Logger instance.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        ///     Analysis output file.
        /// </summary>
        private FileStream analysisFile;

        /// <summary>
        ///     Frame counter.
        /// </summary>
        private long frameCount;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Analyzes an H.264 frame and logs the structure.
        /// </summary>
        /// <param name="frameData">H.264 frame data.</param>
        public void AnalyzeFrame(byte[] frameData)
        {
            if (frameData == null || frameData.Length == 0)
                return;

            try
            {
                frameCount++;
                StringBuilder analysis = new();
                analysis.AppendLine($"=== Frame #{frameCount} ===");
                analysis.AppendLine($"Size: {frameData.Length} bytes");
                analysis.AppendLine(
                    $"First 16 bytes: {BitConverter.ToString(frameData, 0, Math.Min(16, frameData.Length))}");

                // Analyze NAL units
                List<(int type, int offset, int size)> nalUnits = FindNalUnits(frameData);
                analysis.AppendLine($"NAL Units found: {nalUnits.Count}");

                foreach ((int type, int offset, int size) nal in nalUnits)
                {
                    int nalType = nal.type;
                    string nalTypeName = GetNalTypeName(nalType);
                    analysis.AppendLine(
                        $"  NAL Type: {nalType} ({nalTypeName}), Size: {nal.size} bytes, Offset: {nal.offset}");

                    logger?.LogTrace("[H264StreamAnalyzer] Frame {frame}: NAL {type} ({name}), {size} bytes",
                        frameCount, nalType, nalTypeName, nal.size);
                }

                // Check for essential NAL units
                bool hasSPS = nalUnits.Exists(n => n.type == 7);
                bool hasPPS = nalUnits.Exists(n => n.type == 8);
                bool hasIDR = nalUnits.Exists(n => n.type == 5);
                bool hasSlice = nalUnits.Exists(n => n.type == 1);

                analysis.AppendLine($"Essential NAL Units: SPS={hasSPS}, PPS={hasPPS}, IDR={hasIDR}, Slice={hasSlice}");

                if (!hasSPS && !hasPPS)
                {
                    analysis.AppendLine("⚠️ WARNING: Missing SPS/PPS - stream may not be playable");
                }

                analysis.AppendLine();

                // Write to analysis file
                byte[] analysisBytes = Encoding.UTF8.GetBytes(analysis.ToString());
                analysisFile.Write(analysisBytes, 0, analysisBytes.Length);
                analysisFile.Flush();

                // Log summary
                logger?.LogTrace(
                    "[H264StreamAnalyzer] Frame {frame}: {nalCount} NAL units, SPS={sps}, PPS={pps}, IDR={idr}",
                    frameCount, nalUnits.Count, hasSPS, hasPPS, hasIDR);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[H264StreamAnalyzer] Error analyzing frame {frame}", frameCount);
            }
        }

        /// <summary>
        ///     Disposes the analyzer.
        /// </summary>
        public void Dispose()
        {
            analysisFile?.Dispose();
            logger?.LogTrace("[H264StreamAnalyzer] Analysis completed. Total frames: {count}", frameCount);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Finds NAL units in H.264 data.
        /// </summary>
        /// <param name="data">H.264 data.</param>
        /// <returns>List of NAL unit information.</returns>
        private List<(int type, int offset, int size)> FindNalUnits(byte[] data)
        {
            List<(int type, int offset, int size)> nalUnits = new();

            for (int i = 0; i < data.Length - 4; i++)
            {
                // Look for NAL unit start codes: 0x00 0x00 0x00 0x01 or 0x00 0x00 0x01
                bool isStartCode = false;
                int startCodeLength = 0;

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
                    int nalHeaderOffset = i + startCodeLength;
                    if (nalHeaderOffset < data.Length)
                    {
                        int nalType = data[nalHeaderOffset] & 0x1F;

                        // Find next start code to determine size
                        int nextStartCode = FindNextStartCode(data, nalHeaderOffset + 1);
                        int nalSize = nextStartCode > 0
                            ? nextStartCode - nalHeaderOffset
                            : data.Length - nalHeaderOffset;

                        nalUnits.Add((nalType, nalHeaderOffset, nalSize));

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
            for (int i = startOffset; i < data.Length - 4; i++)
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
        ///     Gets the name of a NAL unit type.
        /// </summary>
        /// <param name="nalType">NAL unit type.</param>
        /// <returns>NAL unit type name.</returns>
        private string GetNalTypeName(int nalType)
        {
            return nalType switch
            {
                1 => "Coded slice (non-IDR)",
                5 => "Coded slice (IDR)",
                6 => "SEI",
                7 => "SPS (Sequence Parameter Set)",
                8 => "PPS (Picture Parameter Set)",
                9 => "Access Unit Delimiter",
                12 => "Filler Data",
                _ => $"Unknown ({nalType})"
            };
        }

        #endregion
    }
}