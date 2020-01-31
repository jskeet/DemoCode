﻿// Copyright 2019 Jon Skeet. All rights reserved.
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

        public static Task<RolandMidiClient> CreateRolandMidiClientAsync(MidiInputDevice input, MidiOutputDevice output, DeviceIdentity deviceIdentity, int modelId) =>
            RolandMidiClient.CreateAsync(input, output, deviceIdentity.RawDeviceId, modelId);
    }
}
