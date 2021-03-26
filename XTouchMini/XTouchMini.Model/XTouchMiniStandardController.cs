// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Commons.Music.Midi;
using System.Threading.Tasks;

namespace XTouchMini.Model
{
    /// <summary>
    /// Controller for working with an X-Touch Mini in Standard mode.
    /// </summary>
    public class XTouchMiniStandardController : XTouchMiniController
    {
        private XTouchMiniStandardController(IMidiInput inputPort, IMidiOutput outputPort)
            : base(inputPort, outputPort)
        {
            SetOperationMode(OperationMode.Standard);
        }

        /// <summary>
        /// Connects to an X-Touch Mini and sets it to Standard mode.
        /// </summary>
        /// <param name="name">The MIDI name of the input/output ports.</param>
        public static Task<XTouchMiniStandardController> ConnectAsync(string name) =>
            ConnectAsync(name, (input, output) => new XTouchMiniStandardController(input, output));

        protected override void HandleMidiMessage(byte[] data)
        {
            switch (data[0])
            {
                case 0xba:
                    // Sliders: 0x09 for layer A, 0x0a for layer B
                    if (data[1] == 0x09 || data[1] == 0x0a)
                    {
                        OnFaderMoved(data[1] == 0x09 ? Layer.LayerA : Layer.LayerB, data[2]);
                    }
                    // Knobs, 0x01-0x08 for layer A, 0x0b-0x12 for layer B
                    else
                    {
                        OnKnobTurned(data[1] % 0xa, data[1] < 0x0b ? Layer.LayerA : Layer.LayerB, data[2]);
                    }
                    break;
                case 0x8a:
                case 0x9a:
                    byte note = data[1];
                    bool down = data[0] == 0x9a;
                    if (note < 8)
                    {
                        // Map 0x00-0x07 to knobs 1-8 (layer A)
                        OnKnobPressRelease(note + 1, Layer.LayerA, down);
                    }
                    else if (note < 0x18)
                    {
                        // Map 0x08-0x17 to buttons 1-16 (layer A)
                        OnButtonPressRelease(note - 7, Layer.LayerA, down);
                    }
                    else if (note < 0x20)
                    {
                        // Map 0x18-0x1f to knobs 1-8 (layer B)
                        OnKnobPressRelease(note - 0x17, Layer.LayerB, down);
                    }
                    else
                    {
                        // Map 0x20-0x2f to buttons 1-16 (layer B)
                        OnButtonPressRelease(note - 0x1f, Layer.LayerB, down);
                    }
                    break;
            }
        }

        public void SetActiveLayer(Layer layer) =>
            SendMidiMessage(0xc0, (byte) (layer - 1));
            
        public void SetKnobPosition(int knob, int position) =>
            SendMidiMessage(0xba, (byte) knob, (byte) position);

        public void SetKnobRingStyle(int knob, KnobRingStyle style) =>
            SendMidiMessage(0xb0, (byte) knob, (byte) style);

        /// <summary>
        /// Sets the ring lights for a knob.
        /// </summary>
        /// <param name="knob">The knob to set the lights for</param>
        /// <param name="state">The overall state: off, on, or blinking</param>
        /// <param name="value">The individual value (0 for off, </param>
        public void SetKnobRingLights(int knob, LedState state, int value)
        {
            byte midiValue = (state, value) switch
            {
                (LedState.Off, _) => 0,
                (_, 0) => 0,
                (LedState.On, 14) => 27,
                (LedState.Blinking, 14) => 28,
                (LedState.On, >= 1 and <= 13) => (byte) value,
                (LedState.Blinking, >= 1 and <= 13) => (byte) (value + 13),
                _ => 0
            };
            SendMidiMessage(0xb0, (byte) (knob + 8), midiValue);
        }

        public void SetButtonState(int button, LedState state) =>
            SendMidiMessage(0x90, (byte) (button - 1), (byte) state);
    }
}
