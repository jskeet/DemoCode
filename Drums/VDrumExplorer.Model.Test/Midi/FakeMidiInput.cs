// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Model.Test.Midi
{
    internal class FakeMidiInput : IMidiInput
    {
        public event EventHandler<MidiMessage> MessageReceived;

        public void SupplyMessage(MidiMessage message) => MessageReceived?.Invoke(this, message);

        public void Dispose()
        {
        }
    }
}
