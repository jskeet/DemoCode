// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Commons.Music.Midi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Midi.ManagedMidi
{
    /// <summary>
    /// Implementation of <see cref="IMidiManager"/> using managed-midi.
    /// </summary>
    public class MidiManager : IMidiManager
    {
        public IEnumerable<MidiInputDevice> ListInputDevices() =>
            MidiAccessManager.Default.Inputs
                .Select(port => new MidiInputDevice(port.Id, port.Name, port.Manufacturer));

        public IEnumerable<MidiOutputDevice> ListOutputDevices() =>
            MidiAccessManager.Default.Outputs
                .Select(port => new MidiOutputDevice(port.Id, port.Name, port.Manufacturer));

        public async Task<Model.Midi.IMidiInput> OpenInputAsync(Model.Midi.MidiInputDevice input)
        {
            // Try to open the input up to 3 times. We sometimes see:
            // System.ComponentModel.Win32Exception (4): The specified device is already in use.  Wait until it is free, and then try again. (4)
            int failures = 0;
            while (true)
            {
                try
                {
                    var managedInput = await MidiAccessManager.Default.OpenInputAsync(input.SystemDeviceId);
                    return new MidiInput(managedInput);
                }
                catch when (failures < 3)
                {
                    failures++;
                    await Task.Delay(250);
                }
            }
        }

        public async Task<Model.Midi.IMidiOutput> OpenOutputAsync(Model.Midi.MidiOutputDevice output)
        {
            var managedOutput = await MidiAccessManager.Default.OpenOutputAsync(output.SystemDeviceId);
            return new MidiOutput(managedOutput);
        }
    }
}
