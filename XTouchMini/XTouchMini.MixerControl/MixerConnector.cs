﻿using OscMixerControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XTouchMini.Model;

namespace XTouchMini.MixerControl
{
    internal class MixerConnector
    {
        private readonly List<ControlledChannel> channels;
        private readonly Mixer mixer;
        private readonly XTouchMiniMackieController controller;
        private readonly Timer renewTimer;
        private readonly Channel mainOutput;

        internal MixerConnector(XTouchMiniMackieController controller, Mixer mixer)
        {
            this.mixer = mixer;
            this.controller = controller;
            controller.ButtonDown += HandleButtonDown;
            controller.KnobTurned += HandleKnobTurned;
            controller.FaderMoved += ChangeMainVolume;

            // TODO: Mapping from knob to input channel customization
            channels = Enumerable.Range(1, 8)
                .Select(index => new ControlledChannel(CreateInputChannel(index), controller, index))
                .ToList();
            mainOutput = new Channel(mixer,
                "/lr/config/name",
                "Main",
                "/lr/mix/fader",
                "/meters/5",
                meterIndex: 6,
                meterIndex2: 7,
                "/lr/mix/on");

            Channel CreateInputChannel(int index)
            {
                var prefix = $"/ch/{index:00}";
                return new Channel(mixer,
                    $"{prefix}/config/name",
                    $"Input {index}",
                    $"{prefix}/mix/fader",
                    $"/meters/1",
                    meterIndex: index - 1,
                    meterIndex2: null,
                    $"{prefix}/mix/on");
            }
            renewTimer = new Timer(RefreshSubscriptionsAsync);
        }

        private async void RefreshSubscriptionsAsync(object state)
        {
            await mixer.SendXRemoteAsync();
            await mixer.SendRenewAllAsync();
        }

        private void HandleKnobTurned(object sender, KnobTurnedEventArgs e) =>
            channels[e.Knob - 1].HandleKnobTurned(e.Value);

        private void HandleButtonDown(object sender, ButtonEventArgs e)
        {
            // We only use the top row of buttons.
            if (e.Button >= 1 && e.Button <= 8)
            {
                channels[e.Button - 1].HandleButtonPressed();
            }
        }

        private void ChangeMainVolume(object sender, FaderEventArgs e) =>
            mainOutput.SetFaderLevel(e.Position / 127f);

        internal async Task StartAsync()
        {
            foreach (var channel in channels)
            {
                await channel.StartAsync().ConfigureAwait(false);
            }
            renewTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }
    }
}
