// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Device;
using VDrumExplorer.Model.Midi;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Console
{
    internal sealed class CopyAerophoneStudioSets : ClientCommandBase
    {
        internal static Command Command { get; } = new Command("copy-aerophone-studio-sets")
        {
            Description = "Copies Aerophone preset studio sets (1-100) to user ones",
            Handler = new CopyAerophoneStudioSets()
        };

        protected override async Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, RolandMidiClient client)
        {
            var schema = ModuleSchema.KnownSchemas[ModuleIdentifier.AE10].Value;
            var instrumentField = schema.PhysicalRoot.ResolveField("TemporaryStudioSet/Part[1]/Instrument");
            var instrumentDataField = new EnumDataField((EnumField) instrumentField);
            var deviceController = new DeviceController(client, new ConsoleLogger(console));

            var temporaryKitRoot = schema.LogicalRoot.ResolveNode("TemporaryStudioSet");

            // First 100 presets
            client.SendData(0x01_00_00_05, new byte[] { 64 });
            for (int i = 1; i <= 100; i++)
            {
                console.WriteLine($"Copying studio set {i}");
                client.SendData(0x01_00_00_06, new byte[] { (byte) ((i - 1) & 0x7f) });
                await Task.Delay(40);
                var data = ModuleData.FromLogicalRootNode(temporaryKitRoot);
                await deviceController.LoadDescendants(data.LogicalRoot, null, progressHandler: null, cancellationToken: default);
                await deviceController.SaveDescendants(data.LogicalRoot, schema.GetKitRoot(i).Container.Address, progressHandler: null, cancellationToken: default);
            }

            return 0;
        }
    }
}
