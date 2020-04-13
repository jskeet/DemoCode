// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Commons.Music.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VDrumExplorer.Midi
{
    /// <summary>
    /// MIDI device discovery methods.
    /// </summary>
    public static class MidiDevices
    {
        /// <summary>
        /// Lists all the currently-connected input devices.
        /// </summary>
        public static IReadOnlyList<MidiInputDevice> ListInputDevices() => MidiAccessManager.Default.Inputs
            .Select(port => new MidiInputDevice(port.Id, port.Name, port.Manufacturer))
            .ToList()
            .AsReadOnly();

        /// <summary>
        /// Lists all the currently-connected output devices.
        /// </summary>
        public static IReadOnlyList<MidiOutputDevice> ListOutputDevices() => MidiAccessManager.Default.Outputs
            .Select(port => new MidiOutputDevice(port.Id, port.Name, port.Manufacturer))
            .ToList()
            .AsReadOnly();

        public static async Task<IReadOnlyList<DeviceIdentity>> ListDeviceIdentities(MidiInputDevice input, MidiOutputDevice output, TimeSpan timeout)
        {
            List<DeviceIdentity> identities = new List<DeviceIdentity>();
            using (var client = await RawMidiClient.CreateAsync(input, output, HandleMessage))
            {
                // Identity request message for all devices IDs.
                client.Send(new RawMidiMessage(new byte[] { 0xf0, 0x7e, 0x7f, 0x06, 0x01, 0xf7 }));
                await Task.Delay(timeout);
            }
            return identities.AsReadOnly();
            
            void HandleMessage(RawMidiMessage message)
            {
                // TODO: Handle 17-byte messages for "long" manufacturer IDs
                var data = message.Data;
                if (data.Length == 15 &&
                    data[0] == 0xf0 && // SysEx
                    data[1] == 0x7e && // Universal non-realtime message
                    data[3] == 0x06 && // General information
                    data[4] == 0x02) // Identity reply
                {
                    byte rawDeviceId = data[2];
                    var manufacturerId = (ManufacturerId) data[5];
                    int familyCode = data[6] + (data[7] << 8);
                    int familyNumberCode = data[8] + (data[9] << 8);
                    int revision = data[10] + (data[11] << 8) + (data[12] << 16) + (data[13] << 24);
                    identities.Add(new DeviceIdentity(rawDeviceId, manufacturerId, familyCode, familyNumberCode, revision));
                }
            }
        }

        public static Task<RolandMidiClient> CreateRolandMidiClientAsync(MidiInputDevice input, MidiOutputDevice output, DeviceIdentity deviceIdentity, ModuleIdentifier identifier) =>
            RolandMidiClient.CreateAsync(input, output, deviceIdentity.RawDeviceId, identifier);

        public static async Task<RolandMidiClient?> DetectRolandMidiClientAsync(Action<string> log, IEnumerable<ModuleIdentifier> knownIdentifiers)
        {
            var inputDevices = ListInputDevices();
            foreach (var device in inputDevices)
            {
                log($"Input device: {device.Name}");
            }
            var outputDevices = ListOutputDevices();
            foreach (var device in outputDevices)
            {
                log($"Output device: {device.Name}");
            }
            var commonNames = inputDevices.Select(input => input.Name)
                .Intersect(outputDevices.Select(output => output.Name))
                .OrderBy(x => x)
                .ToList();

            if (commonNames.Count != 1)
            {
                log("No input and output MIDI ports with the same name detected.");
                return null;
            }
            string name = commonNames[0];
            log($"Detecting Roland device for MIDI ports with name {name}");
            var matchedInputs = inputDevices.Where(input => input.Name == name).ToList();
            var matchedOutputs = outputDevices.Where(output => output.Name == name).ToList();
            if (matchedInputs.Count != 1 || matchedOutputs.Count != 1)
            {
                log($"Error: Name {name} matches multiple input or output MIDI ports.");
                return null;
            }
            var identities = await ListDeviceIdentities(matchedInputs[0], matchedOutputs[0], TimeSpan.FromSeconds(1));
            if (identities.Count != 1)
            {
                log($"Error: {(identities.Count == 0 ? "No" : "Multiple")} devices detected for MIDI port {name}.");
                return null;
            }

            var identity = identities[0];
            log($"Detected single device identity {identity}");
            var matchingKeys = knownIdentifiers.Where(sk => sk.FamilyCode == identity.FamilyCode && sk.FamilyNumberCode == identity.FamilyNumberCode).ToList();
            if (matchingKeys.Count != 1)
            {
                log($"Error: {(matchingKeys.Count == 0 ? "No" : "Multiple")} schemas detected for MIDI device.");
                return null;
            }
            log($"Identity matches known schema {matchingKeys[0].Name}. Device detection complete.");
            var client = await CreateRolandMidiClientAsync(matchedInputs[0], matchedOutputs[0], identity, matchingKeys[0]);
            return client;
        }
    }
}
