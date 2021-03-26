// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscMixerControl;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using XTouchMini.Model;

namespace XTouchMini.MixerControl
{
    /// <summary>
    /// Connection between an X-Touch Mini controller and a mixer channel.
    /// </summary>
    internal class ControlledChannel
    {
        private Channel mixerChannel;
        private XTouchMiniMackieController controller;

        // Knob/button index
        private int controllerIndex;

        internal ControlledChannel(Channel mixerChannel, XTouchMiniMackieController controller, int controllerIndex) =>
            (this.mixerChannel, this.controller, this.controllerIndex) =
            (mixerChannel, controller, controllerIndex);

        internal async Task StartAsync()
        {
            mixerChannel.PropertyChanged += HandleChannelPropertyChanged;
            await mixerChannel.RequestDataOnce();
        }

        private void HandleChannelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Channel.On):
                    HandleChannelOnChanged();
                    break;
                case nameof(Channel.FaderLevel):
                    HandleChannelLevelChanged();
                    break;
            }
        }

        private void HandleChannelLevelChanged() =>
            controller.SetKnobRingState(controllerIndex, KnobRingStyle.Fan, (int) (mixerChannel.FaderLevel * 11));

        internal void HandleChannelOnChanged() =>
            controller.SetButtonLedState(controllerIndex, mixerChannel.On == 1 ? LedState.On : LedState.Off);

        private const int Scale = 100;
        internal void HandleKnobTurned(int value)
        {
            int velocity = value >= 0x41 ? -(value - 0x40) : value;
            var currentLevel = mixerChannel.FaderLevel * Scale;
            var newLevel = Math.Max(Math.Min(currentLevel + velocity, Scale), 0);
            mixerChannel.SetFaderLevel(newLevel / Scale);
        }

        public void HandleButtonPressed() =>
            mixerChannel.SetOn(1 - mixerChannel.On);
    }
}
