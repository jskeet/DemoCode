// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Midi;
using MM = ManagedMidi;

namespace VDrumExplorer.Midi.ManagedMidi
{
    /// <summary>
    /// Implementation of <see cref="IMidiManager"/> using managed-midi.
    /// </summary>
    internal class MidiOutput : IMidiOutput
    {
        private bool disposed = false;
        private readonly MM.IMidiOutput managedOutput;

        internal MidiOutput(MM.IMidiOutput managedOutput)
        {
            this.managedOutput = managedOutput;
        }

        public void Send(MidiMessage message)
        {
            managedOutput.Send(message.Data, 0, message.Data.Length, message.Timestamp);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            managedOutput.Dispose();
        }
    }
}
