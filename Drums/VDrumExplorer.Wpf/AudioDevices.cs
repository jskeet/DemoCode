// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Data.Audio;

namespace VDrumExplorer.Wpf
{
    /// <summary>
    /// Simple access to everything we need to do with NAudio.
    /// This isn't terribly nice in terms of encapsulation (including a hard-coded audio format for
    /// recording) but it'll do until we have more requirements.
    /// </summary>
    internal static class AudioDevices
    {
        private const SupportedWaveFormat RequiredSupportedWaveFormat = SupportedWaveFormat.WAVE_FORMAT_48M16;
        private static readonly WaveFormat WaveFormat = new WaveFormat(sampleRate: 48000, channels: 1);

        internal static AudioFormat AudioFormat { get; } = new AudioFormat(WaveFormat.SampleRate, WaveFormat.Channels, WaveFormat.BitsPerSample);

        /// <summary>
        /// Returns a list of audio device names that support recording in the format we need.
        /// </summary>
        internal static List<string> GetInputDeviceNames()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                if (capabilities.SupportsWaveFormat(RequiredSupportedWaveFormat))
                {
                    names.Add(capabilities.ProductName);
                }
            }
            return names;
        }

        /// <summary>
        /// Returns a list of audio device names that support playback in the default recording format.
        /// TODO: What if we load a file that has a different format?
        /// </summary>
        internal static List<string> GetOutputDeviceNames()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                if (capabilities.SupportsWaveFormat(RequiredSupportedWaveFormat))
                {
                    names.Add(capabilities.ProductName);
                }
            }
            return names;
        }

        /// <summary>
        /// Finds the audio input device ID associated with the given name. Using
        /// the name as a key rather than returning pairs earlier means we could persist
        /// a preferred name without relying on the ID being stable.
        /// </summary>
        internal static int? GetAudioInputDeviceId(string name)
        {
            if (name == null)
            {
                return null;
            }
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                if (capabilities.ProductName == name)
                {
                    return i;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the audio output device ID associated with the given name. Using
        /// the name as a key rather than returning pairs earlier means we could persist
        /// a preferred name without relying on the ID being stable.
        /// </summary>
        internal static int? GetAudioOutputDeviceId(string name)
        {
            if (name == null)
            {
                return null;
            }
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                if (capabilities.ProductName == name)
                {
                    return i;
                }
            }
            return null;
        }

        /// <summary>
        /// Records an audio sample using the specified device.
        /// </summary>
        internal static async Task<byte[]> RecordAudio(int deviceId, TimeSpan duration, CancellationToken cancellationToken)
        {
            int expectedBytes = (int) (AudioFormat.BytesPerSecond * duration.TotalSeconds);
            var stream = new MemoryStream();
            var tcs = new TaskCompletionSource<int>();
            using (var input = new WaveIn())
            {
                input.DeviceNumber = deviceId;
                input.WaveFormat = WaveFormat;
                input.DataAvailable += (sender, args) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled();
                        return;
                    }
                    stream.Write(args.Buffer, 0, args.BytesRecorded);
                    if (stream.Length >= expectedBytes)
                    {
                        input.StopRecording();
                        tcs.TrySetResult(0);
                    }
                };
                input.StartRecording();
                await tcs.Task;
            }
            // Truncate to exactly the expected data.
            stream.SetLength(expectedBytes);
            return stream.ToArray();
        }

        /// <summary>
        /// Plays an audio sample on the specified device.
        /// </summary>
        internal static async Task PlayAudio(int deviceId, AudioFormat format, byte[] bytes, CancellationToken cancellationToken)
        {
            double seconds = bytes.Length / (double) format.BytesPerSecond;
            // I've failed to get NAudio to tell me accurately when the sample has finished playing.
            // Instead, let's just pause for long enough - with half a second of added leeway, just
            // in case.
            TimeSpan expectedTime = TimeSpan.FromSeconds(seconds + 0.5);
            var waveFormat = new WaveFormat(format.Frequency, format.Bits, format.Channels);
            using (var output = new WaveOut { DeviceNumber = deviceId })
            {
                var waveProvider = new BufferedWaveProvider(waveFormat) { BufferLength = bytes.Length };
                waveProvider.AddSamples(bytes, 0, bytes.Length);
                output.Init(waveProvider);
                output.Play();
                await Task.Delay(expectedTime, cancellationToken);
            }
        }
    }
}
