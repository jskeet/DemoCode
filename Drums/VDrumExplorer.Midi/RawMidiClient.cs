// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Commons.Music.Midi;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VDrumExplorer.Midi
{
    /// <summary>
    /// A MIDI client with almost no logic
    /// </summary>
    internal sealed class RawMidiClient : IDisposable
    {
        private readonly IMidiInput input;
        private readonly IMidiOutput output;

        private RawMidiClient(IMidiInput input, IMidiOutput output, Action<RawMidiMessage> messageHandler)
        {
            this.input = input;
            this.output = output;
            input.MessageReceived += (sender, args) => messageHandler(ConvertMessage(args));
        }

        private static RawMidiMessage ConvertMessage(MidiReceivedEventArgs args) =>
            args.Length == args.Data.Length && args.Start == 0
            ? new RawMidiMessage(args.Data)
            : new RawMidiMessage(args.Data.Skip(args.Start).Take(args.Length).ToArray());

        internal static async Task<RawMidiClient> CreateAsync(MidiInputDevice inputDevice, MidiOutputDevice outputDevice, Action<RawMidiMessage> messageHandler)
        {
            var input = await MidiAccessManager.Default.OpenInputAsync(inputDevice.SystemDeviceId);
            var output = await MidiAccessManager.Default.OpenOutputAsync(outputDevice.SystemDeviceId);
            return new RawMidiClient(input, output, messageHandler);
        }

        internal void Send(RawMidiMessage message)
        {
            output.Send(message.Data, 0, message.Data.Length, 0L);
        }

        public void Dispose()
        {
            // FIXME
            input.CloseAsync();
            output.CloseAsync();
        }
    }
}
