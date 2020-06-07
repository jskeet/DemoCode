// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NAudio.Wave;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model.Audio;

namespace VDrumExplorer.NAudio
{
    internal class InputDevice : IAudioInput
    {
        private static readonly AudioFormat audioFormat =
            new AudioFormat(NAudioDeviceManager.WaveFormat.SampleRate, NAudioDeviceManager.WaveFormat.Channels, NAudioDeviceManager.WaveFormat.BitsPerSample);

        public string Name { get; }
        public AudioFormat AudioFormat => audioFormat;

        private readonly int id;

        internal InputDevice(string name, int id) =>
            (Name, this.id) = (name, id);

        /// <summary>
        /// Records an audio sample using the specified device.
        /// </summary>
        public async Task<byte[]> RecordAudioAsync(TimeSpan duration, CancellationToken cancellationToken)
        {
            int expectedBytes = (int) (AudioFormat.BytesPerSecond * duration.TotalSeconds);
            var stream = new MemoryStream();
            var tcs = new TaskCompletionSource<int>();
            using (var input = new WaveIn())
            {
                input.DeviceNumber = id;
                input.WaveFormat = NAudioDeviceManager.WaveFormat;
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
    }
}
