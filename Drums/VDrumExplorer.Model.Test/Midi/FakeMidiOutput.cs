// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Model.Test.Midi
{
    internal class FakeMidiOutput : IMidiOutput
    {
        public List<MidiMessage> Messages { get; } = new List<MidiMessage>();

        public void Dispose()
        {
        }

        public void Send(MidiMessage message) => Messages.Add(message);
    }
}
