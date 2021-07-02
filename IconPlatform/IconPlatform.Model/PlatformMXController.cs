using Commons.Music.Midi;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconPlatform.Model
{
    public class PlatformMXController
    {
        public EventHandler<ButtonEventArgs> ButtonChanged;
        public EventHandler<FaderEventArgs> FaderMoved;
        public EventHandler<KnobTurnedEventArgs> KnobTurned;

        public string PortName { get; }
        private IMidiInput inputPort;
        private IMidiOutput outputPort;
        private byte modelId;

        public bool Connected => inputPort is object;

        private PlatformMXController(string portName, byte modelId) =>
            (PortName, this.modelId) = (portName, modelId);

        public static async Task<PlatformMXController> ConnectAsync(string portName)
        {
            var controller = new PlatformMXController(portName, portName.StartsWith("Platform M") ? (byte) 0x14 : (byte) 0x15);
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
            var input = manager.Inputs.FirstOrDefault(p => p.Name == PortName);
            var output = manager.Outputs.FirstOrDefault(p => p.Name == PortName);

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

        public void ClearText()
        {
            SetText(0, new string(' ', 0x38));
            SetText(0x38, new string(' ', 0x38));
        }

        /// <summary>
        /// Sets the text on a Platform D2.
        /// </summary>
        /// <param name="position">The raw position, beginning at 0x00 for the bottom row and 0x38 for the top row.</param>
        /// <param name="text">The text to set. Must be ASCII.</param>
        public void SetText(int position, string text)
        {
            var textBytes = Encoding.ASCII.GetBytes(text);
            var allBytes = new byte[textBytes.Length + 8];
            allBytes[0] = 0xf0;
            allBytes[1] = 0x00;
            allBytes[2] = 0x00;
            allBytes[3] = 0x66; // Mackie control
            allBytes[4] = modelId;
            allBytes[5] = 0x12; // Display command
            allBytes[6] = (byte) position;
            Buffer.BlockCopy(textBytes, 0, allBytes, 7, textBytes.Length);
            allBytes[^1] = 0xf7;
            SendMidiMessage(allBytes);
        }

        /// <summary>
        /// Sets the text for a channel. The first channel has 5 characters available,
        /// and the remainder have 6 each.
        /// </summary>
        /// <param name="channel">The one-based channel number.</param>
        /// <param name="topRow">The top row of text</param>
        /// <param name="bottomRow">The bottom row of text</param>
        public void SetChannelText(int channel, string topRow, string bottomRow)
        {
            int width = GetChannelTextWidth(channel);
            int position = GetChannelTextPosition(channel);
            SetText(0x38 + position, topRow.PadRight(width).Substring(0, width));
            SetText(position, bottomRow.PadRight(width).Substring(0, width));
        }

        /// <summary>
        /// Returns the number of text characters available (per row) for the specified channel.
        /// </summary>
        /// <param name="channel">The one-based channel number.</param>
        /// <returns>The width of the channel's text</returns>
        public int GetChannelTextWidth(int channel) => channel <= 1 ? 5 : 6;

        /// <summary>Returns the left-most position (0-based) of the text for the given channel.</summary>
        /// <param name="channel">The one-based channel number.</param>
        private int GetChannelTextPosition(int channel) =>
            Enumerable.Range(1, channel - 1).Sum(channel => GetChannelTextWidth(channel) + 1);

        /// <summary>
        /// Sets the character in the gap between each channel.
        /// </summary>
        /// <param name="delimiter"></param>
        public void WriteChannelDelimiters(char delimiter)
        {
            string text = delimiter.ToString();
            for (int channel = 2; channel <= 8; channel++)
            {
                var position = GetChannelTextPosition(channel) - 1;
                SetText(position, text);
                SetText(position + 0x38, text);
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
