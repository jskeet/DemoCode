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

namespace VDrumExplorer.Console
{
    class ImportKitCommand : ICommandHandler
    {
        internal static Command Command { get; } = CreateCommand();

        private static Command CreateCommand()
        {
            var command = new Command("import-kit")
            {
                Description = "Imports a kit from a device, saving it as a file",
                Handler = new ImportKitCommand(),
            };
            command.AddOption(new Option("--kit", "Kit number to import") { Argument = new Argument<int>(), Required = true });
            command.AddOption(new Option("--file", "File to save") { Argument = new Argument<string>(), Required = true  });
            return command;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;
            var kit = context.ParseResult.ValueForOption<int>("kit");
            var file = context.ParseResult.ValueForOption<string>("file");
            var (client, schema) = await DeviceDetection.DetectDeviceAsync(console);

            if (client == null)
            {
                return 1;
            }

            var moduleData = new ModuleData();

            using (client)
            {
                if (!schema.KitRoots.TryGetValue(kit, out var kitRoot))
                {
                    console.WriteLine($"Kit {kit} out of range");
                    return 1;
                };

                // Allow up to 30 seconds in total, and 1 second per container.
                var overallToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
                try
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    var containers = kitRoot.Context.AnnotateDescendantsAndSelf().Where(c => c.Container.Loadable).ToList();
                    console.WriteLine($"Loading {containers.Count} containers from device {schema.Identifier.Name}");
                    foreach (var container in containers)
                    {
                        await PopulateSegment(moduleData, container, overallToken);
                    }
                    console.WriteLine($"Finished loading in {(int) sw.Elapsed.TotalSeconds} seconds");
                    var clonedData = kitRoot.Context.CloneData(moduleData, schema.KitRoots[1].Context.Address);
                    var kitToSave = new Kit(schema, clonedData, kit);
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

            async Task PopulateSegment(ModuleData data, AnnotatedContainer annotatedContainer, CancellationToken token)
            {
                var timerToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
                var effectiveToken = CancellationTokenSource.CreateLinkedTokenSource(token, timerToken).Token;
                try
                {
                    var segment = await client.RequestDataAsync(annotatedContainer.Context.Address.Value, annotatedContainer.Container.Size, effectiveToken);
                    data.Populate(annotatedContainer.Context.Address, segment);
                }
                catch (OperationCanceledException) when (timerToken.IsCancellationRequested)
                {
                    console.WriteLine($"Device didn't respond for container {annotatedContainer.Path}; skipping.");
                }
            }
        }
    }
}
