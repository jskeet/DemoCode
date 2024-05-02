// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace XTouchMini.Model
{
    /// <summary>
    /// Controller for working with an X-Touch Mini in Mackie Control mode.
    /// </summary>
    public class XTouchMiniMackieController : XTouchMiniController
    {
        private static readonly byte[] MidiButtons =
        {
            // Placeholder so that the array can be accessed in a 1-based way
            0,
            // Top row
            0x59,
            0x5a,
            0x28,
            0x29,
            0x2a,
            0x2b,
            0x2c,
            0x2d,
            // Bottom row
            0x57,
            0x58,
            0x5b,
            0x5c,
            0x56,
            0x5d,
            0x5e,
            0x5f
        };

        private const byte LayerAMidiButton = 0x54;
        private const byte LayerBMidiButton = 0x55;

        public XTouchMiniMackieController(ILogger logger, string portName) : base(logger, portName)
        {
        }

        public override async Task<bool> MaybeReconnect()
        {
            var result = await base.MaybeReconnect().ConfigureAwait(false);
            if (result)
            {
                SetOperationMode(OperationMode.MackieControl);
            }
            return result;
        }

        protected override void HandleMidiMessage(byte[] data)
        {
            switch (data[0])
            {
                case 0xb0:
                    OnKnobTurned(data[1] - 0x0f, Layer.None, data[2]);
                    break;
                case 0xe8:
                    OnFaderMoved(Layer.None, data[2]);
                    break;
                case 0x90:
                    byte midiButton = data[1];
                    bool down = data[2] == 0x7f;
                    switch (midiButton)
                    {
                        case >= 0x20 and <= 0x27:
                            OnKnobPressRelease(midiButton - 0x1f, Layer.None, down);
                            break;
                        case LayerAMidiButton:
                            OnButtonPressRelease(0, Layer.LayerA, down);
                            break;
                        case LayerBMidiButton:
                            OnButtonPressRelease(0, Layer.LayerB, down);
                            break;
                        default:
                            int button = Array.IndexOf(MidiButtons, midiButton);
                            if (button != -1)
                            {
                                OnButtonPressRelease(button, Layer.None, down);
                            }
                            break;
                    }
                    break;
            }
        }

        public static Task<XTouchMiniMackieController> ConnectAsync(ILogger logger, string portName) =>
            ConnectAsync(new XTouchMiniMackieController(logger, portName));

        public void SetButtonLedState(int button, LedState state)
        {
            byte midiButton = MidiButtons[button];
            SetButtonLedStateImpl(midiButton, state);
        }

        public void SetLayerButtonLedState(Layer layer, LedState state)
        {
            switch (layer)
            {
                case Layer.LayerA:
                    SetButtonLedStateImpl(LayerAMidiButton, state);
                    break;
                case Layer.LayerB:
                    SetButtonLedStateImpl(LayerBMidiButton, state);
                    break;
            }
        }

        private void SetButtonLedStateImpl(byte midiButton, LedState state)
        {
            byte midiState = state switch
            {
                LedState.Off => 0,
                LedState.On => 0x7f,
                LedState.Blinking => 0x01,
                _ => 0
            };
            SendMidiMessage(0x90, midiButton, midiState);
        }

        public void SetKnobRingState(int knob, KnobRingStyle style, int value)
        {
            int midiValue = style switch
            {
                KnobRingStyle.Single => value,
                KnobRingStyle.Trim => value + 16,
                KnobRingStyle.Fan => value + 32,
                KnobRingStyle.Spread => value + 48,
                _ => 0
            };
            SendMidiMessage(0xb0, (byte) (0x2f + knob), (byte) midiValue);
        }

        /// <summary>
        /// Resets all the buttons and knob ring lights to off.
        /// </summary>
        public void Reset()
        {
            for (int i = 1; i <= 8; i++)
            {
                SetKnobRingState(i, KnobRingStyle.Fan, 0);
            }
            for (int i = 1; i <= 16; i++)
            {
                SetButtonLedState(i, LedState.Off);
            }
        }
    }
}
