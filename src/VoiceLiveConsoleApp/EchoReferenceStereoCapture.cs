// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{
    /// <summary>
    ///     Builds the interleaved stereo PCM16 input required by the 2026-06-01-preview client-side echo
    ///     cancellation reference feature (<c>input_audio_echo_cancellation.reference_source = "client"</c>,
    ///     <c>channels = 2</c>).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The service expects <c>[mic₀, ref₀, mic₁, ref₁, …]</c> where channel 0 is the microphone and
    ///         channel 1 is the audio actually played back to the speaker (the echo reference). This class
    ///         isolates that plumbing: the playback path feeds the reference via <see cref="EnqueueReference" />
    ///         and the capture path converts each mono microphone frame into an interleaved stereo frame via
    ///         <see cref="BuildStereoFrame" />.
    ///     </para>
    ///     <para>
    ///         Alignment: what the microphone picks up right now is what the speaker is playing right now,
    ///         which is the audio the client queued minus whatever is still sitting in the playback buffer.
    ///         Response audio arrives in bursts (much faster than real time) and the microphone can be off for
    ///         a while, so a plain FIFO drifts arbitrarily. Instead, the caller passes the current playback
    ///         backlog to <see cref="BuildStereoFrame" />, and the buffer fast-forwards (drops the oldest
    ///         bytes) so that the audio it hands out is the audio that is actually leaving the speaker.
    ///         This is still best-effort — device/driver latency beyond the buffer isn't modeled — so it
    ///         demonstrates the wire mechanics rather than a sample-accurate acoustic alignment. When the
    ///         reference underruns (assistant not speaking) the reference channel is filled with silence.
    ///     </para>
    /// </remarks>
    public sealed class EchoReferenceStereoCapture
    {
        #region Private Fields

        private readonly object gate = new object();

        /// <summary>Circular buffer of recently queued playback bytes (mono PCM16).</summary>
        private readonly byte[] reference;

        /// <summary>Index of the oldest retained byte in <see cref="reference" />.</summary>
        private int head;

        /// <summary>Number of valid bytes currently retained.</summary>
        private int count;

        /// <summary>Total mono samples converted to stereo (i.e. microphone samples sent).</summary>
        private long totalSamples;

        /// <summary>Samples whose reference channel was filled with silence because the buffer underran.</summary>
        private long silenceSamples;

        /// <summary>Samples whose reference channel carried non-zero (audible) playback audio.</summary>
        private long audibleSamples;

        /// <summary>Playback backlog reported by the caller on the most recent frame, in bytes.</summary>
        private int lastBacklogBytes;

        /// <summary>Reference bytes dropped as already-played (fast-forward), for diagnostics.</summary>
        private long droppedBytes;

        /// <summary>
        ///     Frames where the retained history was shorter than the playback backlog, so the reference had to
        ///     hand out audio that has not been played yet (misaligned — echo cancellation can't work).
        /// </summary>
        private long shortHistoryFrames;

        /// <summary>Frames processed (used to report the misaligned ratio).</summary>
        private long frames;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EchoReferenceStereoCapture" /> class.
        /// </summary>
        /// <param name="sampleRate">The mono sample rate in Hz (both mic and reference). Defaults to 24000.</param>
        /// <param name="maxSeconds">
        ///     How much played audio to retain, in seconds. This must cover the playback backlog, because the
        ///     reference for "what the speaker is emitting now" lies that far back in the queued audio.
        ///     Response audio arrives much faster than real time, so the backlog routinely reaches several
        ///     seconds; retaining less than the backlog would force the buffer to hand out audio that has not
        ///     been played yet (i.e. reference from the future, which defeats echo cancellation).
        /// </param>
        public EchoReferenceStereoCapture(int sampleRate = 24000, int maxSeconds = 30)
        {
            // 16-bit mono => 2 bytes per sample.
            reference = new byte[sampleRate * 2 * maxSeconds];
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Appends played-back mono PCM16 audio to the echo reference buffer.
        /// </summary>
        /// <param name="monoPcm">The mono PCM16 audio that was queued for speaker playback.</param>
        public void EnqueueReference(byte[] monoPcm)
        {
            if (monoPcm == null || monoPcm.Length == 0)
            {
                return;
            }

            lock (gate)
            {
                // Keep only the newest bytes if the incoming chunk is larger than the whole buffer.
                int offset = Math.Max(0, monoPcm.Length - reference.Length);
                int length = monoPcm.Length - offset;

                for (int i = 0; i < length; i++)
                {
                    int tail = (head + count) % reference.Length;
                    reference[tail] = monoPcm[offset + i];
                    if (count == reference.Length)
                    {
                        head = (head + 1) % reference.Length;
                    }
                    else
                    {
                        count++;
                    }
                }
            }
        }

        /// <summary>
        ///     Interleaves a mono microphone frame (channel 0) with the time-aligned echo reference
        ///     (channel 1), padding the reference with silence when it underruns.
        /// </summary>
        /// <param name="micMonoPcm">The mono PCM16 microphone frame.</param>
        /// <param name="playbackBacklogBytes">
        ///     How many bytes of previously queued audio are still waiting in the playback buffer (i.e. queued
        ///     but not yet heard). The reference is fast-forwarded so the bytes handed out are the ones leaving
        ///     the speaker now, rather than whatever accumulated while audio arrived in bursts or the
        ///     microphone was off.
        /// </param>
        /// <returns>An interleaved stereo PCM16 frame (twice the input length), or an empty frame if the input
        /// holds less than one sample.</returns>
        public byte[] BuildStereoFrame(byte[] micMonoPcm, int playbackBacklogBytes = 0)
        {
            if (micMonoPcm == null || micMonoPcm.Length < 2)
            {
                return Array.Empty<byte>();
            }

            int sampleCount = micMonoPcm.Length / 2;
            var stereo = new byte[sampleCount * 4];

            lock (gate)
            {
                lastBacklogBytes = playbackBacklogBytes;
                frames++;

                // Read position: the audio leaving the speaker right now sits playbackBacklogBytes behind the
                // newest queued byte. Compute that offset fresh on every frame and only READ from the buffer —
                // never consume it. Consuming would advance the read position at the microphone's rate, which
                // drifts ahead whenever playback lags behind capture (exactly what happens as the backlog
                // grows), and once ahead it can never recover. Recomputing per frame is self-correcting.
                int readOffset = count - playbackBacklogBytes;
                if (readOffset < 0)
                {
                    // We no longer hold audio that far back (buffer too small, or the reference was reset while
                    // playback was still draining): the reference would be ahead of the real echo.
                    shortHistoryFrames++;
                    readOffset = 0;
                }

                for (int i = 0; i < sampleCount; i++)
                {
                    int micByte = i * 2;
                    int outByte = i * 4;

                    // Channel 0: microphone.
                    stereo[outByte] = micMonoPcm[micByte];
                    stereo[outByte + 1] = micMonoPcm[micByte + 1];

                    // Channel 1: echo reference at the aligned position (silence once we run past the newest
                    // byte, i.e. nothing more has been queued for playback).
                    int pos = readOffset + i * 2;
                    if (pos + 1 < count)
                    {
                        stereo[outByte + 2] = reference[(head + pos) % reference.Length];
                        stereo[outByte + 3] = reference[(head + pos + 1) % reference.Length];

                        if (stereo[outByte + 2] != 0 || stereo[outByte + 3] != 0)
                        {
                            audibleSamples++;
                        }
                    }
                    else
                    {
                        stereo[outByte + 2] = 0;
                        stereo[outByte + 3] = 0;
                        silenceSamples++;
                    }

                    totalSamples++;
                }

                // Retire the audio this frame consumed in real time (it has now been played), keeping the
                // buffer bounded without touching the alignment maths above.
                int retire = Math.Min(readOffset, micMonoPcm.Length);
                if (retire > 0)
                {
                    head = (head + retire) % reference.Length;
                    count -= retire;
                    droppedBytes += retire;
                }
            }

            return stereo;
        }

        /// <summary>
        ///     Returns a one-line summary of what the reference channel actually carried, so a run can be
        ///     verified from the log: how much stereo audio was sent, how much of the reference channel held
        ///     real playback audio versus silence, the current playback backlog, and how much already-played
        ///     audio was skipped to stay aligned.
        /// </summary>
        /// <param name="sampleRate">The mono sample rate used to convert byte counts to milliseconds.</param>
        /// <returns>A formatted diagnostics line.</returns>
        public string DescribeStats(int sampleRate = 24000)
        {
            lock (gate)
            {
                double sentSec = totalSamples / (double)sampleRate;
                double audiblePct = totalSamples == 0 ? 0 : 100.0 * audibleSamples / totalSamples;
                double silencePct = totalSamples == 0 ? 0 : 100.0 * silenceSamples / totalSamples;
                double backlogMs = 1000.0 * lastBacklogBytes / 2 / sampleRate;
                double bufferedMs = 1000.0 * count / 2 / sampleRate;
                double skippedSec = droppedBytes / 2.0 / sampleRate;

                string alignment = shortHistoryFrames == 0
                    ? "aligned"
                    : $"short history on {100.0 * shortHistoryFrames / frames:F0}% of frames " +
                      "(reference newer than the audio being played; expected briefly after a barge-in reset)";

                return $"sent {sentSec:F1}s stereo | ref audible {audiblePct:F1}% / silence {silencePct:F1}% " +
                       $"| playback backlog {backlogMs:F0}ms | ref buffered {bufferedMs:F0}ms " +
                       $"| skipped {skippedSec:F1}s | {alignment}";
            }
        }

        /// <summary>
        ///     Clears the reference buffer (e.g. on barge-in or when playback is flushed) so stale audio does
        ///     not desynchronize the reference channel.
        /// </summary>
        public void Reset()
        {
            lock (gate)
            {
                head = 0;
                count = 0;
            }
        }

        #endregion
    }
}
