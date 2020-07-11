// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model.Device;
using VDrumExplorer.Proto;

namespace VDrumExplorer.Console
{
    internal sealed class ImportKitCommand : DeviceCommandBase
    {
        internal static Command Command { get; } = new Command("import-kit")
        {
            Description = "Imports a kit from a device, saving it as a file",
            Handler = new ImportKitCommand(),
        }
        .AddRequiredOption<int>("--kit", "Kit number to import")
        .AddRequiredOption<string>("--file", "File to save");

        protected override async Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, DeviceController device)
        {
            var kitNumber = context.ParseResult.ValueForOption<int>("kit");
            var file = context.ParseResult.ValueForOption<string>("file");

            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                // Allow up to 30 seconds in total.
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
            return 0;
        }
    }
}
