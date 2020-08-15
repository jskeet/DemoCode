// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Threading.Tasks;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Device;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Console
{
    class DumpDeviceSegmentCommand : DeviceCommandBase
    {
        internal static Command Command { get; } = new Command("dump-device-segment")
        {
            Description = "Loads a segment from a device, and dumps the raw bytes of the data in hex ",
            Handler = new DumpDeviceSegmentCommand(),
        }
        .AddRequiredOption<string>("--path", "Path to the logical container")
        .AddOptionalOption<bool>("--interpret", "Whether or not to interpret the data", false);

        protected override async Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, DeviceController device)
        {
            var path = context.ParseResult.ValueForOption<string>("path");
            bool interpret = context.ParseResult.ValueForOption<bool>("interpret");
            var root = device.Schema.LogicalRoot.ResolveNode(path);

            var container = (FieldContainer) root.Container;
            var segment = await device.LoadSegment(container.Address, container.Size, default);
            foreach (var field in container.Fields)
            {
                var bytes = new byte[field.Size];
                segment.ReadBytes(field.Offset, bytes.AsSpan());
                if (field is OverlayField overlay)
                {
                    var sizePerNestedField = field.Size / overlay.NestedFieldCount;
                    for (int i = 0; i < overlay.NestedFieldCount; i++)
                    {
                        string description = $"{field.Description} {i + 1}";
                        var nestedBytes = bytes[(i * sizePerNestedField) .. ((i + 1) * sizePerNestedField)];
                        console.WriteLine($"{description,-30}: {BitConverter.ToString(nestedBytes).Replace('-', ' ')}");
                    }
                }
                else
                {
                    console.WriteLine($"{field.Description,-30}: {BitConverter.ToString(bytes).Replace('-', ' ')}");
                }
            }

            if (interpret)
            {
                console.WriteLine("Interpreted data:");
                var data = ModuleData.FromLogicalRootNode(root);
                var snapshot = new ModuleDataSnapshot();
                snapshot.Add(segment);
                data.LoadSnapshot(snapshot, new ConsoleLogger(console));

                foreach (var field in container.Fields)
                {
                    var dataField = data.GetDataField(field);
                    if (dataField is OverlayDataField overlay)
                    {
                        var list = overlay.CurrentFieldList;
                        console.WriteLine($"Overlay description: {list.Description}");
                        foreach (var subfield in list.Fields)
                        {
                            console.WriteLine($"  {subfield.SchemaField.Description,-28}: {subfield.FormattedText}");
                        }
                    }
                    else
                    {
                        console.WriteLine($"{field.Description,-30}: {dataField.FormattedText}");
                    }
                }
            }

            return 0;
        }
    }
}
