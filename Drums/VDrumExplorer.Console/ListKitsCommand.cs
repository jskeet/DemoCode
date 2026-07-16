// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model.Device;

namespace VDrumExplorer.Console
{
    internal sealed class ListKitsCommand : DeviceCommandBase
    {
        internal static Command Command { get; } = new Command("list-kits")
        {
            Description = "List all the kits on the connected device",
            Action = new ListKitsCommand()
        };

        protected override async Task<int> InvokeAsync(ParseResult parseResult, TextWriter console, DeviceController device)
        {
            var schema = device.Schema;
            for (int i = 1; i <= schema.Kits; i++)
            {
                var name = await device.LoadKitNameAsync(i, CancellationToken.None);
                console.WriteLine($"Kit {i}: {name}");
            }
            return 0;
        }
    }
}
