// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Blazor.WebMidi
{
    internal class WebMidiInput : IMidiInput
    {
        private WebMidiInputProxy proxy;

        public event EventHandler<MidiMessage> MessageReceived;

        internal WebMidiInput(WebMidiInputProxy proxy)
        {
            this.proxy = proxy;
            proxy.MessageReceived += HandleMessageReceived;
        }

        public void Dispose()
        {
            proxy.MessageReceived -= HandleMessageReceived;
        }

        private void HandleMessageReceived(object sender, MidiMessage message) =>
            MessageReceived?.Invoke(this, message);
    }
}
