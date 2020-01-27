// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Console
{
    class ListDevicesCommand : ICommandHandler
    {
        internal static Command Command { get; } = new Command("list-devices")
        {
            Description = "List the devices available",
            Handler = new ListDevicesCommand()
        };

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;
            console.WriteLine("Input devices:");
            var inputDevices = MidiDevices.ListInputDevices();
            foreach (var device in inputDevices)
            {
                console.WriteLine($"{device.SystemDeviceId}: {device.Name}");
            }
            console.WriteLine();
            console.WriteLine("Output devices:");
            var outputDevices = MidiDevices.ListOutputDevices();
            foreach (var device in outputDevices)
            {
                console.WriteLine($"{device.SystemDeviceId}: {device.Name}");
            }
            console.WriteLine();
            var commonNames = inputDevices.Select(input => input.Name)
                .Intersect(outputDevices.Select(output => output.Name))
                .OrderBy(x => x)
                .ToList();

            foreach (var name in commonNames)
            {
                var matchedInputs = inputDevices.Where(input => input.Name == name).ToList();
                var matchedOutputs = outputDevices.Where(output => output.Name == name).ToList();
                if (matchedInputs.Count != 1 || matchedOutputs.Count != 1)
                {
                    continue;
                }
                console.WriteLine($"Detecting device identities for {name}...");
                var identities = await MidiDevices.ListDeviceIdentities(matchedInputs[0], matchedOutputs[0], TimeSpan.FromSeconds(1));
                foreach (var identity in identities)
                {
                    console.WriteLine(identity.ToString());
                }
                console.WriteLine();
            }

            return 0;
        }
    }
}
