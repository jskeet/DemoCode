// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Layout;

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

        public Task<int> InvokeAsync(InvocationContext context)
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
                return Task.FromResult(1);
            }

            if (!(loaded is Kit kit))
            {
                console.WriteLine($"File did not parse as a kit file");
                return Task.FromResult(1);
            }

            var json = ConvertToJson(kit.KitRoot, kit.Data);
            console.WriteLine(json.ToString(Formatting.Indented));
            return Task.FromResult(0);
        }

        private static JObject ConvertToJson(VisualTreeNode container, ModuleData data)
        {
            var ret = new JObject();
            foreach (var child in container.Children)
            {
                var description = child.KitOnlyDescription ?? child.Description;
                ret.Add(description.Format(child.Context, data), ConvertToJson(child, data));
            }
            foreach (var detail in container.Details)
            {
                ret.Add(detail.Description, ConvertToJson(detail, container.Context, data));
            }
            return ret;
        }

        private static JToken ConvertToJson(VisualTreeDetail detail, FixedContainer context, ModuleData data)
        {
            if (detail.DetailDescriptions is object)
            {
                var ret = new JArray();
                foreach (var description in detail.DetailDescriptions)
                {
                    ret.Add(description.Format(context, data));
                }
                return ret;
            }
            else
            {
                var ret = new JObject();
                var container = detail.Container;
                var fields = container.GetPrimitiveFields(data)
                    .Where(f => f.IsEnabled(context, data));
                foreach (var field in fields)
                {
                    ret.Add(field.Description, field.GetText(container, data));
                }
                return ret;
            }
        }
    }
}
