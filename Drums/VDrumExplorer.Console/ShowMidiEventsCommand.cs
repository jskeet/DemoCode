// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Commons.Music.Midi;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Console
{
    public sealed class ShowMidiEventsCommand : ICommandHandler
    {
        internal static Command Command { get; } = new Command("show-midi-events")
        {
            Description = "Shows the data of a kit, as JSON",
            Handler = new ShowMidiEventsCommand(),
        }
        .AddRequiredOption<string>("--device", "MIDI input device to listen to");

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;
            var inputDevices = MidiDevices.ListInputDevices();
            string requestedDeviceName = context.ParseResult.ValueForOption<string>("device");
            var device = inputDevices.FirstOrDefault(d => d.Name == requestedDeviceName);
            if (device is null)
            {
                console.WriteLine($"Device '{requestedDeviceName}' not found.");
                var names = string.Join(", ", inputDevices.Select(d => $"'{d.Name}'"));
                console.WriteLine($"Available devices: {names}");
                return 1;
            }

            // It's slightly annoying to use the underlying MIDI implementation rather than RawMidiClient, but we can always change
            // it if necessary, and RawMidiClient expects input *and* output (and is currently internal).
            using (var input = await MidiAccessManager.Default.OpenInputAsync(device.SystemDeviceId))
            {
                console.WriteLine($"Listening for one minute - or press Ctrl-C to quit.");
                input.MessageReceived += DisplayMessage;
                // Wait for one minute - or until the user kills the process.
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
            return 0;

            void DisplayMessage(object sender, MidiReceivedEventArgs e)
            {
                var end = e.Start + e.Length;
                var data = e.Data[e.Start..end];

                console.WriteLine($"Received: {BitConverter.ToString(data)}");
            }
        }
    }
}
