// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Concentus;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveSDK
{
    /// <summary>
    ///     Handles audio input/output using NAudio for the VoiceLive SDK console application.
    /// </summary>
    internal class AudioHandler : IDisposable
    {
        #region Static Fields and Constants

        /// <summary>
        ///     Audio sample rate in Hz for regular mode.
        /// </summary>
        private const int SampleRate = 24000;

        /// <summary>
        ///     Number of audio channels for regular mode.
        /// </summary>
        private const int Channels = 1;

        /// <summary>
        ///     Bits per audio sample.
        /// </summary>
        private const int BitsPerSample = 16;

        /// <summary>
        ///     Audio sample rate in Hz for Avatar mode (Opus).
        /// </summary>
        private const int AvatarSampleRate = 48000;

        /// <summary>
        ///     Number of audio channels for Avatar mode (Opus).
        /// </summary>
        private const int AvatarChannels = 2;

        #endregion

        #region Private Fields

        private readonly ILogger logger;

        private WaveInEvent? waveIn;
        private WaveOutEvent? waveOut;
        private BufferedWaveProvider? waveProvider;
        private BufferedWaveProvider? avatarWaveProvider;
        private IOpusDecoder? opusDecoder;
        private bool disposed;

        #endregion

        #region Events

        /// <summary>
        ///     Raised when audio data is available from the microphone.
        /// </summary>
        public event Action<byte[]>? OnAudioDataAvailable;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a value indicating whether recording is active.
        /// </summary>
        public bool IsRecording { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether playback is active.
        /// </summary>
        public bool IsPlaying { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AudioHandler" /> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public AudioHandler(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Initializes audio input and output components.
        /// </summary>
        /// <param name="isAvatarMode">Whether to initialize for Avatar mode (48kHz stereo Opus).</param>
        public void Initialize(bool isAvatarMode)
        {
            // Setup audio input (microphone)
            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(SampleRate, BitsPerSample, Channels),
                BufferMilliseconds = 100
            };
            waveIn.DataAvailable += OnWaveInDataAvailable!;
            waveIn.RecordingStopped += OnRecordingStopped!;

            // Setup audio output (speakers)
            waveOut = new WaveOutEvent();

            // Initialize regular audio provider (24kHz, mono, 16-bit)
            waveProvider = new BufferedWaveProvider(new WaveFormat(SampleRate, BitsPerSample, Channels))
            {
                BufferLength = SampleRate * Channels * 2 * 10, // 10 seconds buffer
                DiscardOnBufferOverflow = true
            };

            if (isAvatarMode)
            {
                // Initialize Avatar audio provider (48kHz, stereo, 16-bit)
                avatarWaveProvider =
                    new BufferedWaveProvider(new WaveFormat(AvatarSampleRate, BitsPerSample, AvatarChannels))
                    {
                        BufferLength = AvatarSampleRate * AvatarChannels * 2 * 10,
                        DiscardOnBufferOverflow = true
                    };

                // Initialize Opus decoder
                opusDecoder = OpusCodecFactory.CreateDecoder(AvatarSampleRate, AvatarChannels);
                logger.LogInformation(
                    "Opus decoder initialized for Avatar mode: {sampleRate}Hz, {channels} channels",
                    AvatarSampleRate, AvatarChannels);

                waveOut.Init(avatarWaveProvider);
                logger.LogInformation("Audio initialized for Avatar mode");
            }
            else
            {
                waveOut.Init(waveProvider);
                logger.LogInformation("Audio initialized for regular mode: {sampleRate}Hz, {channels} channel",
                    SampleRate, Channels);
            }
        }

        /// <summary>
        ///     Adds PCM audio data to the playback buffer.
        /// </summary>
        /// <param name="pcmData">The PCM audio data.</param>
        public void AddPlaybackData(byte[] pcmData)
        {
            if (pcmData.Length == 0 || waveProvider == null) return;

            lock (waveProvider)
            {
                waveProvider.AddSamples(pcmData, 0, pcmData.Length);
            }

            // Auto-start playback if not already playing
            if (waveOut != null && waveOut.PlaybackState != PlaybackState.Playing)
            {
                waveOut.Play();
                IsPlaying = true;
            }
        }

        /// <summary>
        ///     Starts recording from the microphone.
        /// </summary>
        public void StartRecording()
        {
            if (IsRecording) return;

            try
            {
                Console.WriteLine("Starting microphone...");
                waveIn?.StartRecording();
                IsRecording = true;
                Console.WriteLine(
                    "Recording Start - Stops automatically when you finish speaking (Manual stop:'R' key)");
            }
            catch (Exception ex)
            {
                logger.LogError("Error starting recording: {Message}", ex.Message);
            }
        }

        /// <summary>
        ///     Stops recording from the microphone.
        /// </summary>
        public void StopRecording()
        {
            if (!IsRecording) return;

            try
            {
                waveIn?.StopRecording();
                IsRecording = false;
                Console.WriteLine("Recording stopped");
            }
            catch (Exception ex)
            {
                logger.LogError("Error stopping recording: {Message}", ex.Message);
            }
        }

        /// <summary>
        ///     Toggles recording on or off.
        /// </summary>
        public void ToggleRecording()
        {
            if (IsRecording)
                StopRecording();
            else
                StartRecording();
        }

        /// <summary>
        ///     Toggles playback on or off.
        /// </summary>
        public void TogglePlayback()
        {
            if (IsPlaying)
                StopPlayback();
            else
                StartPlayback();
        }

        /// <summary>
        ///     Gets the buffered duration of the audio output.
        /// </summary>
        /// <returns>The buffered duration.</returns>
        public TimeSpan GetBufferedDuration()
        {
            return waveProvider?.BufferedDuration ?? TimeSpan.Zero;
        }

        /// <summary>
        ///     Cleans up audio resources.
        /// </summary>
        public void CleanupAvatarAudio()
        {
            if (avatarWaveProvider != null)
            {
                try
                {
                    avatarWaveProvider.ClearBuffer();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error clearing avatar wave provider buffer");
                }

                avatarWaveProvider = null;
            }

            opusDecoder = null;
        }

        #endregion

        #region Private Methods

        private void StartPlayback()
        {
            if (waveOut != null && waveOut.PlaybackState != PlaybackState.Playing)
            {
                try
                {
                    waveOut.Play();
                    IsPlaying = true;
                    Console.WriteLine("Playback started");
                }
                catch (Exception ex)
                {
                    logger.LogError("Error starting playback: {Message}", ex.Message);
                }
            }
        }

        private void StopPlayback()
        {
            if (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
            {
                try
                {
                    waveOut.Stop();
                    IsPlaying = false;
                    Console.WriteLine("Playback stopped");
                }
                catch (Exception ex)
                {
                    logger.LogError("Error stopping playback: {Message}", ex.Message);
                }
            }

            IsPlaying = false;
        }

        private void OnWaveInDataAvailable(object sender, WaveInEventArgs e)
        {
            if (!IsRecording || e.BytesRecorded <= 0) return;

            byte[] audioData = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, 0, audioData, 0, e.BytesRecorded);
            OnAudioDataAvailable?.Invoke(audioData);
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            logger.LogTrace("Recording stopped");
            if (e.Exception != null)
            {
                logger.LogError("Recording error: {Message}", e.Exception.Message);
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        ///     Releases resources used by the audio handler.
        /// </summary>
        public void Dispose()
        {
            if (disposed) return;

            StopRecording();
            StopPlayback();
            CleanupAvatarAudio();

            waveIn?.Dispose();
            waveOut?.Dispose();

            waveIn = null;
            waveOut = null;
            waveProvider = null;

            disposed = true;
        }

        #endregion
    }
}
