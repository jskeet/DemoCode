// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Data.Logical;
using VDrumExplorer.Model.Device;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Console
{
    internal sealed class CheckMfxDefaultsCommand : DeviceCommandBase
    {
        internal static Command Command { get; } = new Command("check-mfx-defaults")
        {
            Description = "Checks the default settings for each MFX option",
            Handler = new CheckMfxDefaultsCommand(),
        }
        .AddRequiredOption<int>("--kit", "Kit number to interact with");

        protected override async Task<int> InvokeAsync(InvocationContext context, IStandardStreamWriter console, DeviceController device)
        {
            var kitNumber = context.ParseResult.ValueForOption<int>("kit");

            // TODO: This is fine for drums, but maybe have the same for an Aerophone?
            if (device.Schema.Identifier.Name != "TD-27")
            {
                console.WriteLine($"MFX checking is only supported on the TD-27 (for simplicity)");
                return 1;
            }

            var mfxNode = device.Schema.GetKitRoot(kitNumber).ResolveNode("MultiFX[1]");
            var mfxContainer = mfxNode.Container;
            var typeField = (EnumField) mfxContainer.ResolveField("Type");
            var parametersField = (OverlayField) mfxContainer.ResolveField("Parameters");

            var deviceData = ModuleData.FromLogicalRootNode(mfxNode);
            var deviceParametersField = (OverlayDataField) deviceData.GetDataField(parametersField);

            console.WriteLine($"Loading original MFX data");
            await device.LoadDescendants(deviceData.LogicalRoot, null, null, default);
            var originalSnapshot = deviceData.CreateSnapshot();

            var modelData = ModuleData.FromLogicalRootNode(mfxNode);
            var modelTypeField = (EnumDataField) modelData.GetDataField(typeField);
            var modelParametersField = (OverlayDataField) modelData.GetDataField(parametersField);

            try
            {
                // Set it to the max value so that we'll be resetting it.
                await SetDeviceMfx(typeField.Max);
                for (int mfxType = typeField.Min; mfxType <= typeField.Max; mfxType++)
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
                    foreach (var (modelField, deviceField) in AsNumericFields(modelFields.Fields).Zip(AsNumericFields(deviceFields.Fields)))
                    {
                        if (modelField.RawValue != deviceField.RawValue)
                        {
                            console.WriteLine($"{modelField.SchemaField.Name}: Device={deviceField.RawValue}; Model={modelField.RawValue}");
                        }
                    }
                    console.WriteLine();
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
                var segment = new DataSegment(mfxContainer.Address + typeField.Offset, new[] { (byte) type });
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