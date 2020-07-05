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
    /// A MIDI client with almost no logic.
    /// </summary>
    internal sealed class RawMidiClient : IDisposable
    {
        public string InputName { get; }
        public string OutputName { get; }

        private readonly IMidiInput input;
        private readonly IMidiOutput output;

        public event EventHandler<RawMidiMessage>? MessageReceived;

        private RawMidiClient(IMidiInput input, string inputName, IMidiOutput output, string outputName)
        {
            this.input = input;
            InputName = inputName;
            this.output = output;
            OutputName = outputName;
            input.MessageReceived += HandleMessage;
        }

        private void HandleMessage(object sender, MidiReceivedEventArgs args)
        {
            MessageReceived?.Invoke(this, ConvertMessage(args));
        }

        private static RawMidiMessage ConvertMessage(MidiReceivedEventArgs args) =>
            args.Length == args.Data.Length && args.Start == 0
            ? new RawMidiMessage(args.Data)
            : new RawMidiMessage(args.Data.Skip(args.Start).Take(args.Length).ToArray());

        internal static async Task<RawMidiClient> CreateAsync(MidiInputDevice inputDevice, MidiOutputDevice outputDevice)
        {
            var input = await OpenInput(inputDevice.SystemDeviceId);
            var output = await MidiAccessManager.Default.OpenOutputAsync(outputDevice.SystemDeviceId);
            return new RawMidiClient(input, inputDevice.Name, output, outputDevice.Name);
        }

        private static async Task<IMidiInput> OpenInput(string deviceId)
        {
            // Try to open the input up to 3 times. We sometimes see:
            // System.ComponentModel.Win32Exception (4): The specified device is already in use.  Wait until it is free, and then try again. (4)
            int failures = 0;
            while (true)
            {
                try
                {
                    return await MidiAccessManager.Default.OpenInputAsync(deviceId);
                }
                catch when (failures < 3)
                {
                    failures++;
                    await Task.Delay(250);
                }
            }
        }

        internal void Send(RawMidiMessage message)
        {
            output.Send(message.Data, 0, message.Data.Length, 0L);
        }

        public void Dispose()
        {
            // Calling CloseAsync is significantly faster than calling Dispose.
            // This is slightly odd, as the implementation for desktop seems to call
            // CloseAsync too. Not sure what's going on, and maybe we should be waiting
            // the task to complete... but it doesn't seem to cause any harm.
            input.CloseAsync();
            output.CloseAsync();
        }
    }
}
