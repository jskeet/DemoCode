// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscCore;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OscMixerControl
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
    public class Channel : INotifyPropertyChanged, IDisposable
    {
        private readonly Mixer mixer;
        private readonly string nameAddress;
        private readonly string faderLevelAddress;
        private readonly string onAddress;
        private readonly string outputMeterAddress;
        private readonly int meterIndex;
        private readonly int? meterIndex2;
        private readonly string defaultName;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates an instance to monitor a specific channel within a mixer.
        /// </summary>
        /// <param name="mixer">The mixer that this channel is part of.</param>
        /// <param name="nameAddress">The OSC address for the name of this channel.</param>
        /// <param name="defaultName">The default name to use when the name hasn't been set on the mixer.</param>
        /// <param name="faderLevelAddress">The OSC address for the fader level of this channel.</param>
        /// <param name="outputMeterAddress">The OSC meter address for the output level of this channel.</param>
        /// <param name="meterIndex">The indexer within the OSC meter for the output of this channel (the left part for stereo channels).</param>
        /// <param name="meterIndex2">The indexer within the OSC meter for the right output of this channel, or null if this is a mono channel.</param>
        /// <param name="onAddress">The OSC address for the on/off (muting) control of this channel, or null if the channel does not support muting.</param>
        public Channel(
            Mixer mixer, string nameAddress, string defaultName, string faderLevelAddress, 
            string outputMeterAddress, int meterIndex, int? meterIndex2, string onAddress)
        {
            this.mixer = mixer;
            this.nameAddress = nameAddress;
            this.defaultName = defaultName;
            // TODO: Should we actually have a default name? Isn't this a view concern?
            Name = defaultName;
            this.faderLevelAddress = faderLevelAddress;
            this.outputMeterAddress = outputMeterAddress;
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

        public async Task SubscribeToData(TimeFactor timeFactor)
        {
            await mixer.SendSubscribeAsync(nameAddress, timeFactor).ConfigureAwait(false);
            await mixer.SendSubscribeAsync(faderLevelAddress, timeFactor).ConfigureAwait(false);
            if (HasOn)
            {
                await mixer.SendSubscribeAsync(onAddress, timeFactor).ConfigureAwait(false);
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

        /// <summary>
        /// Unregisters handler from the mixer, so no further updates will be received.
        /// </summary>
        public void Dispose()
        {
            mixer.RemoveHandler(nameAddress, HandleNameMessage);
            mixer.RemoveHandler(faderLevelAddress, HandleFaderLevelMessage);
            mixer.RemoveHandler(outputMeterAddress, HandleOutputLevelMessage);
            if (onAddress is object)
            {
                mixer.RemoveHandler(onAddress, HandleOnMessage);
            }
        }
    }
}
