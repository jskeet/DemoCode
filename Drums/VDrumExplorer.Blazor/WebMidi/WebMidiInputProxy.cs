// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Blazor.WebMidi
{
    /// <summary>
    /// A proxy to the JavaScript MIDI input.
    /// A single instance of this is created per input port,
    /// and then multiple WebMidiInput instances wrap it.
    /// This ends up with multiple layers of indirection, but makes it easier
    /// to manage the event subscriptions.
    /// </summary>
    internal class WebMidiInputProxy : IDisposable
    {
        private DotNetObjectReference<WebMidiInputProxy> objectProxy;
        private string id;

        internal event EventHandler<MidiMessage> MessageReceived;

        internal WebMidiInputProxy(string id)
        {
            objectProxy = DotNetObjectReference.Create(this);
            this.id = id;
        }

        internal async Task Listen(IJSRuntime runtime)
        {
            await runtime.InvokeVoidAsync("midi.addMessageHandler", id, objectProxy);
        }

        [JSInvokable]
        public void OnMessageReceived(WebMidiMessage message) =>
            MessageReceived?.Invoke(this, message.ToMidiMessage());

        public void Dispose() => objectProxy.Dispose();
    }
}
