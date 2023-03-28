// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Commons.Music.Midi;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace XTouchMini.Model
{
    /// <summary>
    /// Base class for X-Touch Mini controller classes, exposing common events.
    /// Derived classes raise the events, and expose mode-specific operations
    /// for controlling the lights of the X-Touch Mini.
    /// </summary>
    public abstract class XTouchMiniController : IAsyncDisposable
    {
        private readonly string portName;
        private IMidiInput inputPort;
        private IMidiOutput outputPort;

        public bool Connected => inputPort is object;

        public event EventHandler<KnobTurnedEventArgs> KnobTurned;
        public event EventHandler<KnobPressEventArgs> KnobDown;
        public event EventHandler<KnobPressEventArgs> KnobUp;
        public event EventHandler<ButtonEventArgs> ButtonDown;
        public event EventHandler<ButtonEventArgs> ButtonUp;
        public event EventHandler<FaderEventArgs> FaderMoved;

        protected XTouchMiniController(string portName) =>
            this.portName = portName;

        /// <summary>
        /// Sets the operation mode of the X-Touch Mini.
        /// </summary>
        public void SetOperationMode(OperationMode operationMode) =>
            SendMidiMessage(0xb0, 0x7f, (byte) operationMode);

        /// <summary>
        /// Checks whether or not there are ports with the given name. If there are, and the
        /// controller is not currently connected, a new connection is made. If there aren't,
        /// and the controller was previously connected, the existing ports are closed and
        /// the the controller is deemed disconnected.
        /// </summary>
        /// <returns>true if the controller has reconnected (from not being connected)</returns>
        public virtual async Task<bool> MaybeReconnect()
        {
            var manager = MidiAccessManager.Default;
            var input = manager.Inputs.FirstOrDefault(p => p.Name == portName);
            var output = manager.Outputs.FirstOrDefault(p => p.Name == portName);

            bool wasConnected = this.inputPort is object && this.outputPort is object;
            bool nowConnected = input is object && output is object;
            if (wasConnected == nowConnected)
            {
                return false;
            }
            if (wasConnected)
            {
                await DisposeAsync().ConfigureAwait(false);
                return false;
            }

            var inputPort = await manager.OpenInputAsync(input.Id).ConfigureAwait(false);
            var outputPort = await manager.OpenOutputAsync(output.Id).ConfigureAwait(false);
            this.inputPort = inputPort;
            this.outputPort = outputPort;
            inputPort.MessageReceived += HandleInputMessage;
            return true;
        }

        private void HandleInputMessage(object sender, MidiReceivedEventArgs args)
        {
            var data = args.Length == args.Data.Length && args.Start == 0
                ? args.Data : args.Data.Skip(args.Start).Take(args.Length).ToArray();
            if (data.Length == 0)
            {
                return;
            }
            // Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.FFFFFF}: Received bytes: {BitConverter.ToString(data)}");
            HandleMidiMessage(data);
        }

        /// <summary>
        /// Must be overridden in derived classes to process MIDI messages.
        /// </summary>
        /// <param name="data">The MIDI message received. Never empty.</param>
        protected abstract void HandleMidiMessage(byte[] data);

        protected void OnKnobTurned(int knob, Layer layer, int value) =>
            KnobTurned?.Invoke(this, new KnobTurnedEventArgs(knob, layer, value));

        protected void OnKnobPressRelease(int knob, Layer layer, bool down)
        {
            EventHandler<KnobPressEventArgs> handler = down ? KnobDown : KnobUp;
            handler?.Invoke(this, new KnobPressEventArgs(knob, layer, down));
        }

        protected void OnButtonPressRelease(int button, Layer layer, bool down)
        {
            EventHandler<ButtonEventArgs> handler = down ? ButtonDown : ButtonUp;
            handler?.Invoke(this, new ButtonEventArgs(button, layer, down));
        }

        protected void OnFaderMoved(Layer layer, int position) =>
            FaderMoved?.Invoke(this, new FaderEventArgs(layer, position));

        public async ValueTask DisposeAsync()
        {
            await CloseAsync(inputPort).ConfigureAwait(false);
            await CloseAsync(outputPort).ConfigureAwait(false);
            inputPort = null;
            outputPort = null;

            async Task CloseAsync(IMidiPort port)
            {
                if (port is null)
                {
                    return;
                }
                try
                {
                    await port.CloseAsync().ConfigureAwait(false);
                }
                catch
                {
                    // Ignore - this happens more often than we'd like...
                }
            }
        }

        protected static async Task<T> ConnectAsync<T>(T controller) where T : XTouchMiniController
        {
            await controller.MaybeReconnect();
            return controller;
        }

        public void SendMidiMessage(params byte[] data) => outputPort?.Send(data, 0, data.Length, 0L);
    }
}
