// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Data;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Console
{
    class CheckInstrumentDefaultsCommand : ICommandHandler
    {
        internal static Command Command { get; } = new Command("check-instrument-defaults")
        {
            Description = "Checks the default settings (including vedit) from every instrument",
            Handler = new CheckInstrumentDefaultsCommand(),
        }
        .AddRequiredOption<int>("--kit", "Kit number to interact with");

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var console = context.Console.Out;
            var kit = context.ParseResult.ValueForOption<int>("kit");
            var file = context.ParseResult.ValueForOption<string>("file");
            var client = await MidiDevices.DetectSingleRolandMidiClientAsync(console.WriteLine, SchemaRegistry.KnownSchemas.Keys);

            if (client == null)
            {
                return 1;
            }
            var schema = SchemaRegistry.KnownSchemas[client.Identifier].Value;
            if (!schema.KitRoots.TryGetValue(kit, out var kitRoot))
            {
                console.WriteLine($"Kit {kit} out of range");
                return 1;
            };

            var moduleData = new ModuleData();

            // Note: a lot of this logic is copied from SoundRecorderDialog. It should go in a model.
            var instrumentRoot = kitRoot.DescendantNodesAndSelf().FirstOrDefault(node => node.InstrumentNumber == 1);
            if (instrumentRoot == null)
            {
                console.WriteLine($"No instrument root available. Please email a bug report to skeet@pobox.com");
                return 1;
            }

            List<FixedContainer> instrumentContainers = instrumentRoot.DescendantNodesAndSelf()
                .SelectMany(node => node.Details)
                .Select(detail => detail.Container)
                .Where(fc => fc != null)
                .Distinct()
                .ToList();

            var originalData = await LoadContainers();
            var predictedData = originalData.Clone();
            var differences = new List<Difference>();
            using (client)
            {
                var (instrumentFieldContext, instrumentField) =
                    (from ct in instrumentContainers
                     orderby ct.Address
                     from field in ct.Container.Fields
                     where field is InstrumentField
                     select (ct, (InstrumentField) field)).FirstOrDefault();
                if (instrumentFieldContext == null)
                {
                    console.WriteLine($"No instrument field available. Please email a bug report to skeet@pobox.com");
                    return 1;
                }

                try
                {
                    foreach (var instrument in schema.PresetInstruments)
                    {
                        predictedData.Snapshot();
                        // Make the change on the real module and load the data.
                        await RawSetInstrument(instrumentFieldContext, instrumentField, instrument);
                        var realData = await LoadContainers();

                        // Make the change in the model.
                        instrumentField.SetInstrument(instrumentFieldContext, predictedData, instrument);

                        // Compare the two.
                        bool anyDifferences = false;
                        foreach (var container in instrumentContainers)
                        {
                            foreach (var field in container.GetChildren(realData).OfType<NumericField>())
                            {
                                var realValue = field.GetRawValue(container, realData);
                                var predictedValue = field.GetRawValue(container, predictedData);
                                if (realValue != predictedValue)
                                {
                                    anyDifferences = true;
                                    differences.Add(new Difference(instrument, container, field, realValue, predictedValue));
                                }
                            }
                        }
                        console.Write(anyDifferences ? "!" : ".");

                        predictedData.RevertSnapshot();
                    }
                }
                finally
                {
                    await RestoreData();
                }
            }
            foreach (var difference in differences)
            {
                console.WriteLine(difference.ToString());
            }
            console.WriteLine($"Total differences: {differences.Count}");
            return 0;

            async Task<ModuleData> LoadContainers()
            {
                var data = new ModuleData();
                foreach (var container in instrumentContainers)
                {
                    await LoadContainerAsync(data, container);
                }
                return data;
            }

            async Task LoadContainerAsync(ModuleData data, FixedContainer context)
            {
                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
                var segment = await client.RequestDataAsync(context.Address.Value, context.Container.Size, cancellationToken);
                data.Populate(context.Address, segment);
            }

            async Task RestoreData()
            {
                console.WriteLine();
                console.WriteLine("Restoring original data");
                foreach (var segment in originalData.GetSegments())
                {
                    client.SendData(segment.Start.Value, segment.CopyData());
                    await Task.Delay(40);
                }
            }

            // Sets the instrument directly in the module, setting *only* that field.
            async Task RawSetInstrument(FixedContainer context, InstrumentField field, Instrument instrument)
            {
                var address = context.Address + field.Offset;
                byte[] bytes  = {
                    (byte) ((instrument.Id >> 12) & 0xf),
                    (byte) ((instrument.Id >> 8) & 0xf),
                    (byte) ((instrument.Id >> 4) & 0xf),
                    (byte) ((instrument.Id >> 0) & 0xf)
                };
                client.SendData(address.Value, bytes);
                await Task.Delay(40);
            }
        }

        private class Difference
        {
            public Instrument Instrument { get; }
            public FixedContainer Container { get; }
            public IField Field { get; }
            public int RealValue { get; }
            public int PredictedValue { get; }

            public Difference(Instrument instrument, FixedContainer container, IField field, int realValue, int predictedValue) =>
                (Instrument, Container, Field, RealValue, PredictedValue) =
                (instrument, container, field, realValue, predictedValue);

            public override string ToString() =>
                $"{Instrument.Group} / {Instrument.Id} ({Instrument.Name}) - {Container}.{Field.Name}: Real={RealValue}; Predicted={PredictedValue}";
        }
    }
}