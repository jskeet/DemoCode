﻿using DigiMixer.Core;
using System.ComponentModel;

namespace DigiMixer.Controls;

public interface IChannelViewModelBase : INotifyPropertyChanged
{
    ChannelAppearance Appearance { get; }
    string DisplayName { get; }
    string ShortName { get; }

    bool IsStereo { get; }
    MeterLevel Output { get; }
    MeterLevel StereoOutput { get; }
    public MeterLevel PeakOutput { get; }
    public MeterLevel StereoPeakOutput { get; }
    public bool Muted { get; set; }
    public IReadOnlyList<FaderViewModel> Faders { get; }
}
