// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using VDrumExplorer.Model.Device;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Console
{
    /// <summary>
    /// Base class for commands which need a connected device.
    /// </summary>
    internal abstract class DeviceCommandBase : ClientCommandBase
    {
        protected override async Task<int> InvokeAsync(ParseResult parseResult, TextWriter console, RolandMidiClient client)
        {
            using (var device = new DeviceController(client, new ConsoleLogger(console)))
            {
                return await InvokeAsync(parseResult, console, device);
            }
        }

        protected abstract Task<int> InvokeAsync(ParseResult parseResult, TextWriter console, DeviceController device);
    }
}
