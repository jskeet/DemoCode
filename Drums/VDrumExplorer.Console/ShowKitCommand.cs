// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Threading.Tasks;
using VDrumExplorer.Data;

namespace VDrumExplorer.Console
{
    class ShowKitCommand : ICommandHandler
    {
        internal static Command Command { get; } = CreateCommand();

        private static Command CreateCommand()
        {
            var command = new Command("show-kit")
            {
                Description = "Shows the data of a kit, as JSON",
                Handler = new ShowKitCommand(),
            };
            command.AddOption(new Option("--file", "File to load") { Argument = new Argument<string>(), Required = true  });
            return command;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;
            var file = context.ParseResult.ValueForOption<string>("file");

            object loaded;
            try
            {
                using (var stream = File.OpenRead(file))
                {
                    loaded = SchemaRegistry.ReadStream(stream);
                }
            }
            catch (Exception ex)
            {
                console.WriteLine($"Error loading {file}: {ex}");
                return 1;
            }

            if (!(loaded is Kit kit))
            {
                console.WriteLine($"File did not parse as a kit file");
                return 1;
            }

            // TODO: Write actual code to create a JObject from the data.
            console.WriteLine($"Kit number: {kit.DefaultKitNumber}");

            // Just so it's async...
            await Task.Yield();
            return 0;
        }
    }
}
