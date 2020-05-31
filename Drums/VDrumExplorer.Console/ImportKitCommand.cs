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
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Device;
using VDrumExplorer.Proto;

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
            var kitNumber = context.ParseResult.ValueForOption<int>("kit");
            var file = context.ParseResult.ValueForOption<string>("file");
            var client = await MidiDevices.DetectSingleRolandMidiClientAsync(new ConsoleLogger(console), ModuleSchema.KnownSchemas.Keys);
            if (client == null)
            {
                return 1;
            }
            using (var device = new DeviceController(client))
            {
                // Allow up to 30 seconds in total.
                try
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    var token = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
                    var kit = await device.LoadKitAsync(kitNumber, null, token);
                    console.WriteLine($"Finished loading in {(int) sw.Elapsed.TotalSeconds} seconds");
                    using (var stream = File.Create(file))
                    {
                        kit.Save(stream);
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
