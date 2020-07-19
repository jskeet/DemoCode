// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.JSInterop;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Blazor.WebMidi
{
    internal class WebMidiOutput : IMidiOutput
    {
        private readonly IJSRuntime runtime;
        private readonly string port;

        internal WebMidiOutput(IJSRuntime runtime, string port)
        {
            this.runtime = runtime;
            this.port = port;
        }

        public void Send(MidiMessage message)
        {
            // Note that we're not waiting for this to complete. That's okay, as it's
            // expected to be "fire and forget" anyway.
            runtime.InvokeVoidAsync("midi.sendMessage", port, message.Data);
        }

        public void Dispose()
        {
        }
    }
}
