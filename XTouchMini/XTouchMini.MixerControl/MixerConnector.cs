// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscMixerControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XTouchMini.Model;

namespace XTouchMini.MixerControl
{
    internal class MixerConnector : IDisposable
    {
        private readonly List<ControlledChannel> channels;
        private readonly Mixer mixer;
        private readonly Timer renewTimer;
        private readonly Channel mainOutput;

        internal MixerConnector(XTouchMiniMackieController controller, Mixer mixer)
        {
            this.mixer = mixer;
            controller.ButtonDown += HandleButtonDown;
            controller.KnobTurned += HandleKnobTurned;
            controller.FaderMoved += ChangeMainVolume;

            // TODO: Mapping from knob to input channel customization
            channels = Enumerable.Range(1, 8)
                .Select(index => new ControlledChannel(XAir.CreateInputChannel(mixer, index), controller, index))
                .ToList();
            mainOutput = XAir.CreateMainOutputChannel(mixer);
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

        public void Dispose() => renewTimer.Dispose();
    }
}
