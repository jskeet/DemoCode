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
using VDrumExplorer.Model.Midi;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Console
{
    internal sealed class ListAerophoneStudioSets : ClientCommandBase
    {
        internal static Command Command { get; } = new Command("list-aerophone-studio-sets")
        {
            Description = "Selects each Aerophone preset and displays it to the console",
            Handler = new ListAerophoneStudioSets()
        };

        protected override async Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, RolandMidiClient client)
        {
            var schema = ModuleSchema.KnownSchemas[ModuleIdentifier.AE10].Value;
            var instrumentField = schema.PhysicalRoot.ResolveField("TemporaryStudioSet/Part[1]/Instrument");
            var instrumentDataField = new EnumDataField((EnumField) instrumentField);

            // Presets
            client.SendData(0x01_00_00_05, new byte[] { 64 });
            for (int i = 1; i <= 128; i++)
            {
                await ShowStudioSet('P', i);
            }

            // Preset 129 is in a different bank...
            client.SendData(0x01_00_00_05, new byte[] { 65 });
            for (int i = 129; i <= 129; i++)
            {
                await ShowStudioSet('P', i);
            }

            // User sets
            client.SendData(0x01_00_00_05, new byte[] { 0 });
            for (int i = 1; i <= 100; i++)
            {
                await ShowStudioSet('U', i);
            }

            async Task ShowStudioSet(char prefix, int index)
            {
                client.SendData(0x01_00_00_06, new byte[] { (byte)((index - 1) & 0x7f) });
                await Task.Delay(50);

                var common = await client.RequestDataAsync(0x18_00_00_00, 16, CancellationToken.None);
                string name = Encoding.UTF8.GetString(common);
                console.WriteLine($"{prefix}:{index:D3}: {name}");
                for (int partIndex = 0; partIndex < 4; partIndex++)
                {
                    var part = await client.RequestDataAsync(0x18_00_20_00 + partIndex * 0x1_00, 9, CancellationToken.None);
                    if (part[1] != 1)
                    {
                        continue;
                    }
                    // The address doesn't matter, as only the offset is passed.
                    var segment = new DataSegment(ModuleAddress.FromDisplayValue(0), part);
                    instrumentDataField.Load(segment);
                    console.WriteLine($"Part {partIndex + 1}: {instrumentDataField.Value}");
                }
                console.WriteLine();
            }

            return 0;
        }
    }
}
