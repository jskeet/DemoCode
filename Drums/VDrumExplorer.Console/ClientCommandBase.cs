// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Console
{
    /// <summary>
    /// Base class for commands which need a connected device, but as a raw RolandMidiClient.
    /// The client is automatically disposed, then detected at the end of the command.
    /// </summary>
    internal abstract class ClientCommandBase : ICommandHandler
    {
        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;
            var client = await MidiDevices.DetectSingleRolandMidiClientAsync(new ConsoleLogger(console), ModuleSchema.KnownSchemas.Keys);
            if (client == null)
            {
                return 1;
            }
            using (client)
            {
                return await InvokeAsync(context, console, client);
            }
        }

        protected abstract Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, RolandMidiClient client);
    }
}
