// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Commons.Music.Midi;
using System;
using System.Linq;
using System.Threading;
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
        private byte? lastMidiStatus;

        public bool Connected => inputPort is object;

        public event EventHandler<KnobTurnedEventArgs> KnobTurned;
        public event EventHandler<KnobPressEventArgs> KnobDown;
        public event EventHandler<KnobPressEventArgs> KnobUp;
        public event EventHandler<ButtonEventArgs> ButtonDown;
        public event EventHandler<ButtonEventArgs> ButtonUp;
        public event EventHandler<FaderEventArgs> FaderMoved;

        protected XTouchMiniController(string portName)
        {
            inputCallback = HandleInputMessage;
            syncContext = SynchronizationContext.Current;
            this.portName = portName;
        }

        /// <summary>
        /// Sets the operation mode of the X-Touch Mini.
        /// </summary>
        public void SetOperationMode(OperationMode operationMode) =>
            SendMidiMessage(0xb0, 0x7f, (byte) operationMode);

        private readonly SynchronizationContext syncContext;
        private readonly SendOrPostCallback inputCallback;

        /// <summary>
        /// Checks whether or not there are ports with the given name. If there are, and the
        /// controller is not currently connected, a new connection is made. If there aren't,
        /// and the controller was previously connected, the existing ports are closed and
        /// the the controller is deemed disconnected. This method does not throw any exceptions
        /// if the port is found but can't be connected.
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
            try
            {
                var inputPort = await manager.OpenInputAsync(input.Id).ConfigureAwait(false);
                var outputPort = await manager.OpenOutputAsync(output.Id).ConfigureAwait(false);
                this.inputPort = inputPort;
                this.outputPort = outputPort;
                this.lastMidiStatus = null;
                // Ensure we process the message in a suitable synchronization context, if we have one.
                inputPort.MessageReceived += (sender, args) =>
                {
                    if (syncContext is null)
                    {
                        inputCallback(args);
                    }
                    else
                    {
                        syncContext.Post(inputCallback, args);
                    }
                };
            }
            catch
            {
                // Deliberately swallow the exception. The port may be in use by another app, which
                // is equivalent to not existing, as far as we're concerned.
                // (This approach is always worrying, but for now it's probably the simplest option.)
                this.inputPort = null;
                this.outputPort = null;
                return false;
            }
            return true;
        }

        private void HandleInputMessage(object state)
        {
            var args = (MidiReceivedEventArgs) state;
            if (args.Length == 0)
            {
                return;
            }

            // Workaround for https://github.com/atsushieno/alsa-sharp/issues/2
            bool useCachedStatus = args.Data[args.Start] < 128;
            if (useCachedStatus && lastMidiStatus is null)
            {
                throw new InvalidOperationException("Received MIDI message with no status byte, and no cached status");
            }
            int dataOffset = useCachedStatus ? 1 : 0;
            int length = args.Length + dataOffset;
            byte[] data = new byte[length];
            if (useCachedStatus)
            {
                data[0] = lastMidiStatus.Value;
            }
            Buffer.BlockCopy(args.Data, args.Start, data, dataOffset, args.Length);
            // Cache the status (regardless of whether we've just received it or not).
            lastMidiStatus = data[0];
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
