// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;
using VDrumExplorer.Data;
using VDrumExplorer.Midi;

namespace VDrumExplorer.Console
{
    internal static class DeviceDetection
    {
        internal static async Task<(RolandMidiClient client, ModuleSchema schema)> DetectDeviceAsync(IStandardStreamWriter console)
        {
            var inputDevices = MidiDevices.ListInputDevices();
            var outputDevices = MidiDevices.ListOutputDevices();
            var commonNames = inputDevices.Select(input => input.Name)
                .Intersect(outputDevices.Select(output => output.Name))
                .OrderBy(x => x)
                .ToList();

            if (commonNames.Count != 1)
            {
                console.WriteLine("Error: No input and output MIDI ports with the same name detected.");
                return (null, null);
            }
            string name = commonNames[0];
            var matchedInputs = inputDevices.Where(input => input.Name == name).ToList();
            var matchedOutputs = outputDevices.Where(output => output.Name == name).ToList();
            if (matchedInputs.Count != 1 || matchedOutputs.Count != 1)
            {
                console.WriteLine($"Error: Name {name} matches multiple input or output MIDI ports.");
                return (null, null);
            }
            var identities = await MidiDevices.ListDeviceIdentities(matchedInputs[0], matchedOutputs[0], TimeSpan.FromSeconds(1));
            if (identities.Count != 1)
            {
                console.WriteLine($"Error: {(identities.Count == 0 ? "No" : "Multiple")} devices detected for MIDI port {name}.");
                return (null, null);
            }

            var schemaKeys = SchemaRegistry.KnownSchemas.Keys;
            var identity = identities[0];

            var matchingKeys = schemaKeys.Where(sk => sk.FamilyCode == identity.FamilyCode && sk.FamilyNumberCode == identity.FamilyNumberCode).ToList();
            if (matchingKeys.Count != 1)
            {
                console.WriteLine($"Error: {(matchingKeys.Count == 0 ? "No" : "Multiple")} schemas detected for MIDI device.");
                return (null, null);
            }
            var schema = SchemaRegistry.KnownSchemas[matchingKeys[0]];
            var moduleIdentifier = schema.Value.Identifier;

            var client = await MidiDevices.CreateRolandMidiClientAsync(matchedInputs[0], matchedOutputs[0], identity, moduleIdentifier.ModelId);
            return (client, schema.Value);
        }
    }
}
