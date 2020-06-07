// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model.Audio;

namespace VDrumExplorer.NAudio
{
    internal sealed class OutputDevice : IAudioOutput
    {
        public string Name { get; }

        private readonly int id;

        internal OutputDevice(string name, int id) =>
            (Name, this.id) = (name, id);

        /// <summary>
        /// Plays an audio sample
        /// </summary>
        public async Task PlayAudioAsync(AudioFormat format, byte[] bytes, CancellationToken cancellationToken)
        {
            double seconds = bytes.Length / (double) format.BytesPerSecond;
            // I've failed to get NAudio to tell me accurately when the sample has finished playing.
            // Instead, let's just pause for long enough - with half a second of added leeway, just
            // in case.
            TimeSpan expectedTime = TimeSpan.FromSeconds(seconds + 0.5);
            var waveFormat = new WaveFormat(format.Frequency, format.Bits, format.Channels);
            using (var output = new WaveOut { DeviceNumber = id })
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
