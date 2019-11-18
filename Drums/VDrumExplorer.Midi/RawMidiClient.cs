// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Sanford.Multimedia.Midi;
using System;

namespace VDrumExplorer.Midi
{
    /// <summary>
    /// A MIDI client with almost no logic
    /// </summary>
    internal sealed class RawMidiClient : IDisposable
    {
        private readonly InputDevice input;
        private readonly OutputDevice output;

        internal RawMidiClient(MidiInputDevice inputDevice, MidiOutputDevice outputDevice, Action<RawMidiMessage> messageHandler)
        {
            input = new InputDevice(inputDevice.SystemDeviceId);
            output = new OutputDevice(outputDevice.SystemDeviceId);
            input.MessageReceived += message => messageHandler(new RawMidiMessage(message.GetBytes()));
            input.StartRecording();
        }

        internal void Send(RawMidiMessage message)
        {
            var status = message.Status;
            if (message.Status >= 0x80 && message.Status < 0xf0)
            {
                var command = (ChannelCommand) (status & 0xf0);
                var channel = status & 0xf;
                if (message.Data.Length == 3)
                {
                    output.Send(new ChannelMessage(command, channel, message.Data[1], message.Data[2]));
                    return;
                }
                else if (message.Data.Length == 2)
                {
                    output.Send(new ChannelMessage(command, channel, message.Data[1]));
                    return;
                }
            }
            if (message.Status == 0xf0)
            {
                output.Send(new SysExMessage(message.Data));
                return;
            }
            throw new ArgumentException($"Invalid or unhandled data: {BitConverter.ToString(message.Data)}");
        }

        public void Dispose()
        {
            input.Dispose();
            output.Dispose();
        }
    }
}
