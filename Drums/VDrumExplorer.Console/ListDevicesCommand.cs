// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Console
{
    internal sealed class ListDevicesCommand : ICommandHandler
    {
        internal static Command Command { get; } = new Command("list-devices")
        {
            Description = "List the devices available",
            Handler = new ListDevicesCommand()
        };

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;
            var clients = await MidiDevices.DetectRolandMidiClientsAsync(new ConsoleLogger(console), ModuleSchema.KnownSchemas.Keys).ToListAsync();
            foreach (var client in clients)
            {
                client.Dispose();
            }
            return 0;
        }
    }
}
