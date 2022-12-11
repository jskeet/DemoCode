// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using NodaTime;
using OscMixerControl.Wpf.Models;
using System.Threading.Tasks;

namespace OscMixerControl.Wpf.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public LogViewModel LogViewModel { get; }
        public CommandViewModel CommandViewModel { get; }

        public MixerViewModel MixerViewModel { get; }
        public Log Log { get; }
        public ILogger Logger => Log.Logger;

        private string mixerAddress = "192.168.1.41";
        public string MixerAddress
        {
            get => mixerAddress;
            set => SetProperty(ref mixerAddress, value);
        }

        private int mixerPort = 10024;
        public int MixerPort
        {
            get => mixerPort;
            set => SetProperty(ref mixerPort, value);
        }

        public MainWindowViewModel()
        {
            Log = new Log(SystemClock.Instance);
            LogViewModel = new LogViewModel(Log);
            CommandViewModel = new CommandViewModel(Logger);
            MixerViewModel = new MixerViewModel(Logger);
        }

        public async Task ConnectAsync()
        {
            await MixerViewModel.ConnectAsync(MixerAddress, MixerPort);
            CommandViewModel.Connect(MixerAddress, MixerPort);
        }

        public Task SendCommandAsync() => CommandViewModel.SendCommandAsync();
    }
}
