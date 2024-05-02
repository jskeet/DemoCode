// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using IconPlatform.Model;
using OscMixerControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IconPlatform.MixerControl
{
    internal class MixerConnector : IDisposable
    {
        private readonly List<Channel> channels;
        private readonly Mixer mixer;
        private readonly Timer renewTimer;
        private readonly PlatformMXController controller;

        internal MixerConnector(PlatformMXController controller, Mixer mixer)
        {
            this.controller = controller;
            this.mixer = mixer;
            controller.ButtonChanged += HandleButtonChanged;
            controller.FaderMoved += ChangeVolume;

            // TODO: Mapping from knob to input channel customization
            channels = Enumerable.Range(1, 8)
                .Select(index => XAirDescriptor.Instance.CreateInputChannel(mixer, index))
                .ToList();
            renewTimer = new Timer(RefreshSubscriptionsAsync);
        }

        private async void RefreshSubscriptionsAsync(object state)
        {
            await mixer.SendXRemoteAsync();
            await mixer.SendRenewAllAsync();
        }

        private void HandleButtonChanged(object sender, ButtonEventArgs e)
        {
            if (e.Button == ButtonType.Mute && e.Down)
            {
                var channel = channels[e.Channel - 1];
                channel.SetOn(1 - channel.On);
            }
        }

        private void ChangeVolume(object sender, FaderEventArgs e) =>
            channels[e.Channel - 1].SetFaderLevel(e.Position / 1023f);

        internal async Task StartAsync()
        {
            for (int i = 0; i < channels.Count; i++)
            {
                int oneChannel = i + 1;
                var channel = channels[i];
                channel.PropertyChanged += (sender, args) =>
                {
                    switch (args.PropertyName)
                    {
                        case nameof(Channel.On):
                            controller.SetLight(oneChannel, ButtonType.Mute, channel.On == 1 ? false : true);
                            break;
                        case nameof(Channel.FaderLevel):
                            controller.MoveFader(oneChannel, (int) (channel.FaderLevel * 1023));
                            break;
                    }
                };
                await channel.RequestDataOnce().ConfigureAwait(false);
            }
            renewTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public void Dispose() => renewTimer.Dispose();
    }
}
