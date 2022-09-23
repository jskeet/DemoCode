// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Console
{
    internal sealed class SendMidiEventsCommand : ICommandHandler
    {
        internal static Command Command { get; } = new Command("send-midi-events")
        {
            Description = "Sends MIDI events specified in hex",
            Handler = new SendMidiEventsCommand(),
        }
        .AddRequiredOption<string>("--device", "MIDI input device to send to");

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var consoleOut = context.Console.Out;
            var outputDevices = MidiDevices.ListOutputDevices();
            string requestedDeviceName = context.ParseResult.ValueForOption<string>("device");
            var device = outputDevices.FirstOrDefault(d => d.Name == requestedDeviceName);
            if (device is null)
            {
                consoleOut.WriteLine($"Device '{requestedDeviceName}' not found.");
                var names = string.Join(", ", outputDevices.Select(d => $"'{d.Name}'"));
                consoleOut.WriteLine($"Available devices: {names}");
                return 1;
            }

            // It's slightly annoying to use the underlying MIDI implementation rather than RawMidiClient, but we can always change
            // it if necessary, and RawMidiClient expects input *and* output (and is currently internal).
            using var output = await MidiDevices.Manager.OpenOutputAsync(device);

            consoleOut.WriteLine("Enter messages as space-separated hex lines, e.g. \"b0 0f 50\"");
            // TODO: Hook into System.CommandLine for this? It doesn't seem to handle interactive aspects at the moment.
            while (System.Console.ReadLine() is string line && line != "")
            {
                var data = line.Split(' ').Select(section => Convert.ToByte(section, 16)).ToArray();
                consoleOut.WriteLine($"Sending {BitConverter.ToString(data)}");
                output.Send(new MidiMessage(data));
            }
            return 0;
        }
    }
}
