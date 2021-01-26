// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OscMixerControl.Wpf.Models
{
    /// <summary>
    /// Model for a channel, consisting of:
    /// - the fader level
    /// - a name
    /// - the meter level
    /// - whether or not it's muted (optional)
    /// 
    /// This can be used for main channels and buses.
    /// </summary>
    public class Channel : INotifyPropertyChanged
    {
        private readonly Mixer mixer;
        private readonly string nameAddress;
        private readonly string faderLevelAddress;
        private readonly string onAddress;
        private readonly int meterIndex;
        private readonly int? meterIndex2;
        private readonly string defaultName;

        public event PropertyChangedEventHandler PropertyChanged;

        public Channel(
            Mixer mixer, string nameAddress, string defaultName, string faderLevelAddress, 
            string outputMeterAddress, int meterIndex, int? meterIndex2, string onAddress)
        {
            this.mixer = mixer;
            this.nameAddress = nameAddress;
            this.defaultName = defaultName;
            Name = defaultName;
            this.faderLevelAddress = faderLevelAddress;
            this.meterIndex = meterIndex;
            this.meterIndex2 = meterIndex2;
            this.onAddress = onAddress;
            mixer.RegisterHandler(nameAddress, HandleNameMessage);
            mixer.RegisterHandler(faderLevelAddress, HandleFaderLevelMessage);
            mixer.RegisterHandler(outputMeterAddress, HandleOutputLevelMessage);
            if (onAddress is object)
            {
                mixer.RegisterHandler(onAddress, HandleOnMessage);
            }
        }

        /// <summary>
        /// The level of the fader, as set by the user.
        /// </summary>
        public float FaderLevel { get; private set; }

        /// <summary>
        /// The current output of the channel.
        /// (Always non-positive; divide by 256.0 to get to dB.)
        /// </summary>
        public short Output { get; private set; }

        /// <summary>
        /// The current second output of the channel (usually for the right
        /// side if the left side is in <see cref="Output"/>).
        /// (Always non-positive; divide by 256.0 to get to dB.)
        /// </summary>
        public short Output2 { get; private set; }

        public bool HasOutput2 => meterIndex2.HasValue;

        public string Name { get; private set; }

        public int On { get; private set; }

        public bool HasOn => onAddress is object;

        public Task SetName(string value) =>
            mixer.SendAsync(new OscMessage(nameAddress, value));

        public Task SetFaderLevel(float value) =>
            mixer.SendAsync(new OscMessage(faderLevelAddress, value));

        public Task SetOn(int value) =>
            mixer.SendAsync(new OscMessage(onAddress, value));

        private void HandleNameMessage(object sender, OscMessage message)
        {
            string newName = (string)message[0];
            Name = newName == "" ? defaultName : newName;
            RaisePropertyChanged(nameof(Name));
        }

        private void HandleOutputLevelMessage(object sender, OscMessage message)
        {
            var blob = (byte[]) message[0];
            Output = BitConverter.ToInt16(blob, meterIndex * 2 + 4);
            RaisePropertyChanged(nameof(Output));
            if (meterIndex2 is int index2)
            {
                Output2 = BitConverter.ToInt16(blob, index2 * 2 + 4);
                RaisePropertyChanged(nameof(Output2));
            }
        }

        private void HandleFaderLevelMessage(object sender, OscMessage message)
        {
            FaderLevel = (float) message[0];
            RaisePropertyChanged(nameof(FaderLevel));
        }

        private void HandleOnMessage(object sender, OscMessage message)
        {
            On = (int) message[0];
            RaisePropertyChanged(nameof(On));
        }

        private void RaisePropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task SubscribeToData()
        {
            await mixer.SendSubscribeAsync(nameAddress, TimeFactor.Medium).ConfigureAwait(false);
            await mixer.SendSubscribeAsync(faderLevelAddress, TimeFactor.Medium).ConfigureAwait(false);
            if (HasOn)
            {
                await mixer.SendSubscribeAsync(onAddress, TimeFactor.Medium).ConfigureAwait(false);
            }
        }

        public async Task RequestDataOnce()
        {
            await mixer.SendAsync(new OscMessage(nameAddress)).ConfigureAwait(false);
            await mixer.SendAsync(new OscMessage(faderLevelAddress)).ConfigureAwait(false);
            if (HasOn)
            {
                await mixer.SendAsync(new OscMessage(onAddress)).ConfigureAwait(false);
            }
        }
    }
}
