// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Model.Device;

namespace VDrumExplorer.Console
{
    /// <summary>
    /// Base class for commands which need a connected device.
    /// </summary>
    internal abstract class DeviceCommandBase : ClientCommandBase
    {
        protected override async Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, RolandMidiClient client)
        {
            using (var device = new DeviceController(client))
            {
                return await InvokeAsync(context, console, device);
            }
        }

        protected abstract Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, DeviceController device);
    }
}
