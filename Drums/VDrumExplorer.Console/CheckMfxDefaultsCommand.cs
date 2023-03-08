// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.
#if NETCOREAPP3_1
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Device;
using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Console
{
    internal sealed class CheckMfxDefaultsCommand : DeviceCommandBase
    {
        internal static Command Command { get; } = new Command("check-mfx-defaults")
        {
            Description = "Checks the default settings for each MFX option",
            Handler = new CheckMfxDefaultsCommand(),
        }
        .AddRequiredOption<string>("--path", "Path to logical node containing MFX (e.g. '/Kits/Kit[100]/MultiFX[1]')")
        .AddOptionalOption("--switch", "Field name for switch", "Type")
        .AddOptionalOption("--parameters", "Field name for parameters", "Parameters");

        protected override async Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, DeviceController device)
        {
            var path = context.ParseResult.ValueForOption<string>("path");
            var switchFieldName = context.ParseResult.ValueForOption<string>("switch");
            var parametersFieldName = context.ParseResult.ValueForOption<string>("parameters");

            var mfxNode = device.Schema.LogicalRoot.ResolveNode(path);
            var mfxContainer = mfxNode.Container;
            var switchField = (EnumField) mfxContainer.ResolveField(switchFieldName);
            var parametersField = (OverlayField) mfxContainer.ResolveField(parametersFieldName);

            var deviceData = ModuleData.FromLogicalRootNode(mfxNode);
            var deviceParametersField = (OverlayDataField) deviceData.GetDataField(parametersField);

            console.WriteLine($"Loading original MFX data");
            await device.LoadDescendants(deviceData.LogicalRoot, null, null, default);
            var originalSnapshot = deviceData.CreateSnapshot();

            var modelData = ModuleData.FromLogicalRootNode(mfxNode);
            var modelTypeField = (EnumDataField) modelData.GetDataField(switchField);
            var modelParametersField = (OverlayDataField) modelData.GetDataField(parametersField);

            try
            {
                // Set it to the max value so that we'll be resetting it.
                await SetDeviceMfx(switchField.Max);
                for (int mfxType = switchField.Min; mfxType <= switchField.Max; mfxType++)
                {
                    // Make the change on the device...
                    await SetDeviceMfx(mfxType);
                    await device.LoadDescendants(deviceData.LogicalRoot, null, null, default);

                    // Make the change in the model...
                    modelTypeField.RawValue = mfxType;

                    var modelFields = modelParametersField.CurrentFieldList;
                    var deviceFields = deviceParametersField.CurrentFieldList;

                    if (modelFields.Description != deviceFields.Description)
                    {
                        console.WriteLine($"Mismatch in description: '{modelFields.Description}' != '{deviceFields.Description}'. Skipping.");
                        continue;
                    }

                    console.WriteLine($"Comparing fields for {modelFields.Description}");
                    bool anyDifferences = false;
                    foreach (var (modelField, deviceField) in AsNumericFields(modelFields.Fields).Zip(AsNumericFields(deviceFields.Fields)))
                    {
                        if (modelField.RawValue != deviceField.RawValue)
                        {
                            console.WriteLine($"{modelField.SchemaField.Name}: Device={deviceField.RawValue}; Model={modelField.RawValue}");
                            anyDifferences = true;
                        }
                    }
                    if (anyDifferences)
                    {
                        console.WriteLine();
                    }
                }
            }
            finally
            {
                // Restore the original data
                console.WriteLine($"Restoring original kit data");
                deviceData.LoadSnapshot(originalSnapshot, NullLogger.Instance);
                await device.SaveDescendants(deviceData.LogicalRoot, targetAddress: null, progressHandler: null, CancellationToken.None);
            }
            return 0;

            async Task SetDeviceMfx(int type)
            {
                var segment = new DataSegment(mfxContainer.Address + switchField.Offset, new[] { (byte) type });
                await device.SaveSegment(segment, CancellationToken.None);
            }

            IEnumerable<NumericDataFieldBase> AsNumericFields(IEnumerable<IDataField> fields)
            {
                foreach (var field in fields)
                {
                    switch (field)
                    {
                        case NumericDataFieldBase numeric:
                            yield return numeric;
                            break;
                        case TempoDataField tempo:
                            yield return tempo.SwitchDataField;
                            yield return tempo.NumericDataField;
                            yield return tempo.MusicalNoteDataField;
                            break;
                        default:
                            throw new InvalidOperationException($"Can't convert {field.GetType()} into a numeric field");
                    }
                }
            }
        }
    }
}
#endif