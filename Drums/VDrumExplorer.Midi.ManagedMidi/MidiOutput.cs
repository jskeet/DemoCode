// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Midi.ManagedMidi
{
    /// <summary>
    /// Implementation of <see cref="IMidiManager"/> using managed-midi.
    /// </summary>
    internal class MidiOutput : IMidiOutput
    {
        private readonly Commons.Music.Midi.IMidiOutput managedOutput;

        internal MidiOutput(Commons.Music.Midi.IMidiOutput managedOutput)
        {
            this.managedOutput = managedOutput;
        }

        public void Send(MidiMessage message)
        {
            managedOutput.Send(message.Data, 0, message.Data.Length, message.Timestamp);
        }

        public void Dispose()
        {
            // Calling CloseAsync is significantly faster than calling Dispose.
            // This is slightly odd, as the implementation for desktop seems to call
            // CloseAsync too. Not sure what's going on, and maybe we should be waiting
            // the task to complete... but it doesn't seem to cause any harm.
            managedOutput.CloseAsync();
        }
    }
}
