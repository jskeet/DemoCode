// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Data.Logical;

namespace VDrumExplorer.Console
{
    internal sealed class ShowKitCommand : ICommandHandler
    {
        internal static Command Command { get; } = new Command("show-kit")
        {
            Description = "Shows the data of a kit, as JSON",
            Handler = new ShowKitCommand(),
        }
        .AddRequiredOption<string>("--file", "File to load");

        public Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;
            var file = context.ParseResult.ValueForOption<string>("file");

            object loaded;
            try
            {
                loaded = Proto.ProtoIo.LoadModel(file);
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

            var dataNode = new DataTreeNode(kit.Data, kit.KitRoot);
            var json = ConvertToJson(dataNode);
            console.WriteLine(json.ToString(Formatting.Indented));
            return Task.FromResult(0);
        }

        private static JObject ConvertToJson(DataTreeNode container)
        {
            var ret = new JObject();
            foreach (var child in container.Children)
            {
                ret.Add(child.SchemaNode.Name, ConvertToJson(child));
            }
            foreach (var detail in container.Details)
            {
                ret.Add(detail.Description, ConvertToJson(detail));
            }
            return ret;
        }

        private static JToken ConvertToJson(IDataNodeDetail detail)
        {
            if (detail is ListDataNodeDetail list)
            {
                var ret = new JArray();
                foreach (var item in list.Items)
                {
                    ret.Add(item.Text);
                }
                return ret;
            }
            else if (detail is FieldContainerDataNodeDetail fieldContainer)
            {
                var ret = new JObject();
                foreach (var field in fieldContainer.Fields)
                {
                    ret.Add(field.SchemaField.Name, field.FormattedText);
                }
                return ret;
            }
            else
            {
                throw new ArgumentException($"Unexpected detail type: {detail.GetType()}");
            }
        }
    }
}
