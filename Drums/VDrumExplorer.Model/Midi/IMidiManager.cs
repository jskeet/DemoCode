// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace VDrumExplorer.Model.Midi
{
    /// <summary>
    /// Abstraction of what V-Drum Explorer needs in terms of enumerating and opening MIDI connections.
    /// </summary>
    public interface IMidiManager
    {
        IEnumerable<MidiInputDevice> ListInputDevices();
        IEnumerable<MidiOutputDevice> ListOutputDevices();
        Task<IMidiInput> OpenInputAsync(MidiInputDevice input);
        Task<IMidiOutput> OpenOutputAsync(MidiOutputDevice output);
    }
}
