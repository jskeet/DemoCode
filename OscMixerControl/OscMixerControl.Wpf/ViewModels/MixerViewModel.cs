// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using OscMixerControl.Wpf.Models;
using System;
using System.Collections.Generic;
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
            Mixer = new Mixer(Dispatcher.CurrentDispatcher);
            Mixer.PacketReceived += (sender, packet) =>
            {
                if (LogPackets)
                {
                    logger.LogPacket(packet);
                }
            };

            var inputChannels = new List<ChannelViewModel>();
            for (int i = 1; i <= 5; i++)
            {
                var prefix = $"/ch/{i:00}";
                var channel = new Channel(Mixer,
                    $"{prefix}/config/name",
                    $"Input {i}",
                    $"{prefix}/mix/fader",
                    $"/meters/1",
                    meterIndex: i - 1,
                    meterIndex2: null,
                    $"{prefix}/mix/on");
                inputChannels.Add(new ChannelViewModel(channel));
            }
            InputChannels = inputChannels;

            // Outputs
            var outputChannels = new List<ChannelViewModel>();
            for (int i = 1; i <= 7; i++)
            {
                var prefix = i == 7 ? "/lr" : $"/bus/{i}";
                var channel = new Channel(Mixer,
                    $"{prefix}/config/name",
                    i == 7 ? "Main" : $"Bus {i}",
                    $"{prefix}/mix/fader",
                    $"/meters/5",
                    meterIndex: i - 1,
                    meterIndex2: i == 7 ? 7 : (int?) null,
                    $"{prefix}/mix/on");
                outputChannels.Add(new ChannelViewModel(channel));
            }
            OutputChannels = outputChannels;
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
            await Mixer.SendBatchSubscribeAsync("/meters/1", "/meters/1", 0, 0, TimeFactor.Medium);
            await Mixer.SendBatchSubscribeAsync("/meters/5", "/meters/5", 0, 0, TimeFactor.Medium);
        }

        async void RefreshSubscriptionsAsync(object sender, EventArgs e)
        {
            await Mixer.SendXRemoteAsync();
            await Mixer.SendRenewAllAsync();
        }
    }
}
