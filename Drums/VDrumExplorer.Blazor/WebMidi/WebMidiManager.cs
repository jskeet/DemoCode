// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Blazor.WebMidi
{
    public class WebMidiManager : IMidiManager
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

        private Dictionary<MidiInputDevice, WebMidiInputProxy> inputs;
        private Dictionary<MidiOutputDevice, WebMidiOutput> outputs;

        internal WebMidiManager(Dictionary<MidiInputDevice, WebMidiInputProxy> inputs,
            Dictionary<MidiOutputDevice, WebMidiOutput> outputs)
        {
            this.inputs = inputs;
            this.outputs = outputs;
        }

        // TODO: Protect against multiple instantiation calls.
        public static async Task<WebMidiManager> InitializeAsync(IJSRuntime runtime)
        {
            var handler = new PromiseHandler();
            await runtime.InvokeAsync<object>("midi.initialize", Timeout, handler.Proxy);
            await handler.Task;

            var inputs = (await runtime.InvokeAsync<List<WebMidiPort>>("midi.getInputPorts", Timeout))
                .Select(input => input.ToMidiInputDevice())
                .ToDictionary(input => input, input => new WebMidiInputProxy(input.SystemDeviceId));

            var outputs = (await runtime.InvokeAsync<List<WebMidiPort>>("midi.getOutputPorts", Timeout))
                .Select(output => output.ToMidiOutputDevice())
                .ToDictionary(input => input, input => new WebMidiOutput(runtime, input.SystemDeviceId));

            foreach (var proxy in inputs.Values)
            {
                await proxy.Listen(runtime);
            }

            return new WebMidiManager(inputs, outputs);
        }

        public IEnumerable<MidiInputDevice> ListInputDevices() => inputs.Keys;

        public IEnumerable<MidiOutputDevice> ListOutputDevices() => outputs.Keys;

        public Task<IMidiInput> OpenInputAsync(MidiInputDevice input) =>
            Task.FromResult((IMidiInput) new WebMidiInput(inputs[input]));

        public Task<IMidiOutput> OpenOutputAsync(MidiOutputDevice output) =>
            Task.FromResult((IMidiOutput) outputs[output]);
    }
}
