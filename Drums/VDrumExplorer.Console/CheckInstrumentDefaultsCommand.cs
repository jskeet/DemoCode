// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Data.Logical;
using VDrumExplorer.Model.Device;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Console
{
    internal sealed class CheckInstrumentDefaultsCommand : DeviceCommandBase
    {
        internal static Command Command { get; } = new Command("check-instrument-defaults")
        {
            Description = "Checks the default settings (including vedit) from every instrument",
            Handler = new CheckInstrumentDefaultsCommand(),
        }
        .AddRequiredOption<int>("--kit", "Kit number to interact with");

        protected override async Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, DeviceController device)
        {
            var kit = context.ParseResult.ValueForOption<int>("kit");
            var file = context.ParseResult.ValueForOption<string>("file");
            var triggerRoot = device.Schema.GetTriggerRoot(kit, trigger: 1);
            var deviceData = ModuleData.FromLogicalRootNode(triggerRoot);
            var modelData = ModuleData.FromLogicalRootNode(triggerRoot);
            var defaultValuesSnapshot = modelData.CreateSnapshot();

            var deviceDataRoot = new DataTreeNode(deviceData, triggerRoot);
            await device.LoadDescendants(deviceDataRoot, targetAddress: null, progressHandler: null, CancellationToken.None);
            var originalSnapshot = deviceData.CreateSnapshot();
            var (ifContainer, instrumentField) = device.Schema.GetMainInstrumentField(kit, trigger: 1);
            var modelInstrumentField = (InstrumentDataField) modelData.GetDataField(ifContainer, instrumentField);

            var instrumentContainers = triggerRoot.DescendantFieldContainers();

            var differences = new List<Difference>();
            try
            {
                // Reset the device to an empty snapshot
                deviceData.LoadSnapshot(defaultValuesSnapshot);
                await device.SaveDescendants(deviceDataRoot, targetAddress: null, progressHandler: null, CancellationToken.None);

                foreach (var instrument in device.Schema.PresetInstruments)
                {
                    // Make the change on the real module and load the data.
                    // Assumption: the segment containing the instrument itself (e.g. KitPadInst) doesn't
                    // have any implicit model changes to worry about.
                    await device.SetInstrumentAsync(kit, trigger: 1, instrument, CancellationToken.None);
                    await device.LoadDescendants(deviceDataRoot, targetAddress: null, progressHandler: null, CancellationToken.None);

                    // Make the change in the model.
                    modelData.LoadSnapshot(defaultValuesSnapshot);
                    modelInstrumentField.Instrument = instrument;

                    // Compare the two.
                    bool anyDifferences = false;
                    foreach (var container in instrumentContainers)
                    {
                        // We won't compare InstrumentDataField, TempoDataField or StringDataField this way, but that's okay.
                        var realFields = deviceData.GetDataFields(container).SelectMany(ExpandOverlays).OfType<NumericDataFieldBase>().ToList();
                        var modelFields = modelData.GetDataFields(container).SelectMany(ExpandOverlays).OfType<NumericDataFieldBase>().ToList();
                        if (realFields.Count != modelFields.Count)
                        {
                            console.WriteLine($"Major failure: for instrument {instrument.Id} ({instrument.Group} / {instrument.Name}), found {realFields.Count} real fields and {modelFields.Count} model fields in container {container.Path}");
                            return 1;
                        }

                        foreach (var pair in realFields.Zip(modelFields))
                        {
                            var real = pair.First;
                            var model = pair.Second;
                            if (real.SchemaField != model.SchemaField)
                            {
                                console.WriteLine($"Major failure: for instrument {instrument.Id} ({instrument.Group} / {instrument.Name}), mismatched schema field for {container.Path}: {real.SchemaField.Name} != {model.SchemaField.Name}");
                                return 1;
                            }
                            var realValue = real.RawValue;
                            var predictedValue = model.RawValue;
                            if (realValue != predictedValue)
                            {
                                anyDifferences = true;
                                differences.Add(new Difference(instrument, container, real.SchemaField, realValue, predictedValue));
                            }
                        }
                    }
                    console.Write(anyDifferences ? "!" : ".");
                }
            }
            finally
            {
                // Restore the original data
                deviceData.LoadSnapshot(originalSnapshot);
                await device.SaveDescendants(deviceDataRoot, targetAddress: null, progressHandler: null, CancellationToken.None);
            }
            console.WriteLine();
            foreach (var difference in differences)
            {
                console.WriteLine(difference.ToString());
            }
            console.WriteLine($"Total differences: {differences.Count}");
            return 0;

            IEnumerable<IDataField> ExpandOverlays(IDataField field) =>
                field is OverlayDataField odf ? odf.CurrentFieldList.Fields : Enumerable.Repeat(field, 1);
        }

        private class Difference
        {
            public Instrument Instrument { get; }
            public IField Field { get; }
            public FieldContainer Container { get; }
            public int RealValue { get; }
            public int PredictedValue { get; }

            public Difference(Instrument instrument, FieldContainer container, IField field, int realValue, int predictedValue) =>
                (Instrument, Container, Field, RealValue, PredictedValue) =
                (instrument, container, field, realValue, predictedValue);

            public override string ToString() =>
                $"{Instrument.Group} / {Instrument.Id} ({Instrument.Name}) - {Container.Path}/{Field.Name}: Real={RealValue}; Predicted={PredictedValue}";
        }
    }
}