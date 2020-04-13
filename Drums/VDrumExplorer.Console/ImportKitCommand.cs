// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Console
{
    class ImportKitCommand : ICommandHandler
    {
        internal static Command Command { get; } = new Command("import-kit")
        {
            Description = "Imports a kit from a device, saving it as a file",
            Handler = new ImportKitCommand(),
        }
        .AddRequiredOption<int>("--kit", "Kit number to import")
        .AddRequiredOption<string>("--file", "File to save");

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;
            var kit = context.ParseResult.ValueForOption<int>("kit");
            var file = context.ParseResult.ValueForOption<string>("file");
            var client = await MidiDevices.DetectRolandMidiClientAsync(console.WriteLine, SchemaRegistry.KnownSchemas.Keys);

            if (client == null)
            {
                return 1;
            }
            var schema = SchemaRegistry.KnownSchemas[client.Identifier].Value;

            using (client)
            {
                if (!schema.KitRoots.TryGetValue(kit, out _))
                {
                    console.WriteLine($"Kit {kit} out of range");
                    return 1;
                };

                // Allow up to 30 seconds in total, and 1 second per container.
                var overallToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
                try
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    var kitToSave = await KitUtilities.ReadKit(schema, client, kit, console);
                    console.WriteLine($"Finished loading in {(int) sw.Elapsed.TotalSeconds} seconds");
                    using (var stream = File.Create(file))
                    {
                        kitToSave.Save(stream);
                    }
                    console.WriteLine($"Saved kit to {file}");
                }
                catch (OperationCanceledException)
                {
                    console.WriteLine("Data loading from device was cancelled");
                    return 1;
                }
                catch (Exception ex)
                {
                    console.WriteLine($"Error loading data from device: {ex}");
                    return 1;
                }

            }
            return 0;
        }
    }
}
