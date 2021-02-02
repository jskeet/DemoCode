// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using OscCore;
using OscMixerControl.Wpf.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace OscMixerControl.Wpf.ViewModels
{
    public class CommandViewModel : ViewModelBase
    {
        private Mixer mixer;

        private string address;
        public string Address
        {
            get => address;
            set => SetProperty(ref address, value);
        }

        public ObservableCollection<CommandParameterViewModel> Parameters { get; }

        public CommandViewModel(ILogger logger)
        {
            mixer = new Mixer();
            mixer.PacketReceived += (sender, packet) => logger.LogPacket(packet);

            Parameters = new ObservableCollection<CommandParameterViewModel>();
            for (int i = 1; i <= 5; i++)
            {
                Parameters.Add(new CommandParameterViewModel($"Parameter {i}"));
            }
        }

        private OscPacket ToOscPacket()
        {
            string addr = address?.Trim();
            if (string.IsNullOrEmpty(addr))
            {
                return null;
            }
            return new OscMessage(addr, Parameters.Select(p => p.Value).Where(v => v is object).ToArray());
        }

        public async Task SendCommandAsync()
        {
            var packet = ToOscPacket();
            if (packet != null)
            {
                await mixer.SendAsync(packet);
            }
        }

        public void Connect(string address, int port) => mixer.Connect(address, port);
    }
}
