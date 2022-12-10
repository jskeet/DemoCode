// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace OscMixerControl.Wpf.ViewModels
{
    public class MixerViewModel : ViewModelBase
    {
        private readonly ILogger logger;
        private DispatcherTimer renewTimer;
        private Mixer Mixer { get; }

        public IReadOnlyList<ChannelViewModel> InputChannels { get; }
        public IReadOnlyList<ChannelViewModel> OutputChannels { get; }

        private bool logPackets;
        public bool LogPackets
        {
            get => logPackets;
            set => SetProperty(ref logPackets, value);
        }

        public MixerViewModel(ILogger logger)
        {
            this.logger = logger;
            renewTimer = new DispatcherTimer(TimeSpan.FromSeconds(5), DispatcherPriority.Normal, RefreshSubscriptionsAsync, Dispatcher.CurrentDispatcher);
            // TODO: Allow the user to specify this, or detect it.
            Mixer = new Mixer(XAirDescriptor.Instance);
            Mixer.PacketReceived += (sender, packet) =>
            {
                if (LogPackets)
                {
                    logger.LogPacket(packet);
                }
            };

            InputChannels = Enumerable.Range(1, 5)
                .Select(index => new ChannelViewModel(Mixer.Descriptor.CreateInputChannel(Mixer, index), $"Input {index}"))
                .ToList();

            // Outputs
            OutputChannels = Enumerable.Range(1, 6)
                .Select(index => new ChannelViewModel(Mixer.Descriptor.CreateAuxOutputChannel(Mixer, index), $"Bus {index}"))
                .Concat(new[] { new ChannelViewModel(Mixer.Descriptor.CreateMainOutputChannel(Mixer), "Main") })
                .ToList();
        }

        public async Task ConnectAsync(string address, int port)
        {
            Mixer.Connect(address, port);
            logger.LogInformation("Connected to {address}:{port}", address, port);
            foreach (var channelVm in InputChannels)
            {
                await channelVm.RequestDataOnce().ConfigureAwait(false);
            }
            foreach (var channelVm in OutputChannels)
            {
                await channelVm.RequestDataOnce().ConfigureAwait(false);
            }
            await Mixer.SendBatchSubscribeAsync(Mixer.Descriptor.InputChannelLevelsMeter, Mixer.Descriptor.InputChannelLevelsMeter, 0, 0, TimeFactor.Medium);
            await Mixer.SendBatchSubscribeAsync(Mixer.Descriptor.OutputChannelLevelsMeter, Mixer.Descriptor.OutputChannelLevelsMeter, 0, 0, TimeFactor.Medium);
        }

        private async void RefreshSubscriptionsAsync(object sender, EventArgs e)
        {
            await Mixer.SendXRemoteAsync();
            await Mixer.SendRenewAllAsync();
        }
    }
}
