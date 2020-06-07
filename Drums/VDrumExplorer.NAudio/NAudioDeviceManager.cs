// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NAudio.Wave;
using System.Collections.Generic;
using VDrumExplorer.Model.Audio;

namespace VDrumExplorer.NAudio
{
    /// <summary>
    /// Implementation of IAudioDeviceManager using NAudio.
    /// </summary>
    public sealed class NAudioDeviceManager : IAudioDeviceManager
    {
        private const SupportedWaveFormat RequiredSupportedWaveFormat = SupportedWaveFormat.WAVE_FORMAT_48M16;
        internal static WaveFormat WaveFormat { get; } = new WaveFormat(sampleRate: 48000, channels: 1);

        /// <summary>
        /// Returns a list of audio device names that support recording in the format we need.
        /// </summary>
        public IReadOnlyList<IAudioInput> GetInputs()
        {
            List<IAudioInput> inputs = new List<IAudioInput>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                if (capabilities.SupportsWaveFormat(RequiredSupportedWaveFormat))
                {
                    inputs.Add(new InputDevice(capabilities.ProductName, i));
                }
            }
            return inputs.AsReadOnly();
        }

        /// <summary>
        /// Returns a list of audio device names that support playback in the default recording format.
        /// TODO: What if we load a file that has a different format?
        /// </summary>
        public IReadOnlyList<IAudioOutput> GetOutputs()
        {
            List<IAudioOutput> outputs = new List<IAudioOutput>();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                if (capabilities.SupportsWaveFormat(RequiredSupportedWaveFormat))
                {
                    outputs.Add(new OutputDevice(capabilities.ProductName, i));
                }
            }
            return outputs;
        }
    }
}
