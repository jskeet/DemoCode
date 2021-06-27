using Commons.Music.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IconPlatform.Model
{
    public class PlatformMXController
    {
        public EventHandler<ButtonEventArgs> ButtonChanged;
        public EventHandler<FaderEventArgs> FaderMoved;
        public EventHandler<KnobTurnedEventArgs> KnobTurned;

        private readonly string portName;
        private IMidiInput inputPort;
        private IMidiOutput outputPort;

        public bool Connected => inputPort is object;

        private PlatformMXController(string portName)
        {
            this.portName = portName;
        }

        public static async Task<PlatformMXController> ConnectAsync(string portName)
        {
            var controller = new PlatformMXController(portName);
            await controller.MaybeReconnect();
            return controller;
        }

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
            int? zeroChannel = (data[0], data[1]) switch
            {
                (0x90, var c) => c % 8,
                (0xb0, var c) => c % 8,
                (var ec, _) when ec >= 0xe0 && ec < 0xe8 => ec - 0xe0,
                _ => null
            };

            if (zeroChannel is null)
            {
                return;
            }

            int oneChannel = zeroChannel.Value + 1;

            switch (data[0])
            {
                case 0x90:
                    var button = MidiToButton(data[1]);
                    if (button is ButtonType)
                    {
                        ButtonChanged?.Invoke(this, new ButtonEventArgs(oneChannel, button.Value, data[2] == 0x7f));
                    }
                    break;
                case >= 0xe0 and < 0xe8:
                    FaderMoved?.Invoke(this, new FaderEventArgs(oneChannel, data[1] / 16 + data[2] * 8));
                    break;
                case 0xb0:
                    KnobTurned?.Invoke(this, new KnobTurnedEventArgs(oneChannel, data[2]));
                    break;
            }
        }

        /// <summary>
        /// Moves the physical fader to the given position.
        /// </summary>
        /// <param name="channel">The one-based channel number.</param>
        /// <param name="position">The position, in the range 0-1023</param>
        public void MoveFader(int channel, int position)
        {
            byte[] data =
            {
                (byte) (0xe0 + channel - 1), // Pitch bend (E0-EF)
                (byte) ((position % 8) * 16),  // Fine control (00, 10, 20 etc)
                (byte) (position / 8)          // Coarse control (00-7F)
            };
            SendMidiMessage(data);
        }

        /// <summary>
        /// Turns the given button light on or off.
        /// </summary>
        /// <param name="channel">The one-based channel number.</param>
        /// <param name="button">The button to turn on or off.</param>
        /// <param name="on">Whether the button should be lit or not.</param>
        public void SetLight(int channel, ButtonType button, bool on)
        {
            byte[] data =
            {
                0x90, // Note on/off
                ButtonToMidi(channel - 1, button),
                on ? (byte) 0x7f : (byte) 0x00
            };
            SendMidiMessage(data);
        }

        /// <summary>
        /// Must be overridden in derived classes to process MIDI messages.
        /// </summary>
        /// <param name="data">The MIDI message received. Never empty.</param>
        private void HandleMidiMessage(byte[] data)
        {
        }
        /*
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
        */
        public async ValueTask DisposeAsync()
        {
            await CloseAsync(inputPort).ConfigureAwait(false);
            await CloseAsync(outputPort).ConfigureAwait(false);
            inputPort = null;
            outputPort = null;

            async Task CloseAsync(IMidiPort port)
            {
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

        public void SendMidiMessage(params byte[] data) => outputPort?.Send(data, 0, data.Length, 0L);

        private static byte ButtonToMidi(int zeroChannel, ButtonType type) =>
            type switch
            {
                ButtonType.Record => (byte)zeroChannel,
                ButtonType.Solo => (byte)(0x08 + zeroChannel),
                ButtonType.Mute => (byte)(0x10 + zeroChannel),
                ButtonType.Sel => (byte)(0x18 + zeroChannel),
                ButtonType.Knob => (byte)(0x20 + zeroChannel),
                ButtonType.Fader => (byte)(0x60 + zeroChannel),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown button type")
            };

        private static ButtonType? MidiToButton(byte value) =>
            value switch
            {
                >= 0x00 and < 0x08 => ButtonType.Record,
                >= 0x08 and < 0x10 => ButtonType.Solo,
                >= 0x10 and < 0x18 => ButtonType.Mute,
                >= 0x18 and < 0x20 => ButtonType.Sel,
                >= 0x20 and < 0x28 => ButtonType.Knob,
                >= 0x68 and < 0x70 => ButtonType.Fader,
                _ => null
            };
    }
}
