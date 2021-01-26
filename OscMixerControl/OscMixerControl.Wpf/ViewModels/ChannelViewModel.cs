// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using OscMixerControl.Wpf.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OscMixerControl.Wpf.ViewModels
{
    public class ChannelViewModel : ViewModelBase<Channel>
    {
        private static readonly Dictionary<string, string> ModelToViewModelPropertyMap = new Dictionary<string, string>
        {
            { nameof(Channel.Name), nameof(Name) },
            { nameof(Channel.FaderLevel), nameof(FaderLevel) },
            { nameof(Channel.Output), nameof(Output) },
            { nameof(Channel.Output2), nameof(Output2) },
            { nameof(Channel.On), nameof(Muted) },
        };

        public string Name
        {
            get => Model.Name;
            set => Model.SetName(value);
        }

        public short Output => Model.Output;
        public short Output2 => Model.Output2;
        public bool HasOutput2 => Model.HasOutput2;

        public int FaderLevel
        {
            get => (int) (Model.FaderLevel * 1024);
            set => Model.SetFaderLevel(value / 1024f);
        }

        public bool Muted
        {
            get => Model.On == 0;
            set => Model.SetOn(value ? 0 : 1);
        }

        public bool HasMute => Model.HasOn;

        public ChannelViewModel(Channel channel) : base(channel)
        {
        }

        protected override void OnPropertyModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ModelToViewModelPropertyMap.TryGetValue(e.PropertyName, out var name))
            {
                RaisePropertyChanged(name);
            }            
        }

        public Task SubscribeToData() => Model.SubscribeToData();
        public Task RequestDataOnce() => Model.RequestDataOnce();
    }
}
