using DigiMixer.Core;
using System.ComponentModel;

namespace DigiMixer.AppCore;

public interface IChannelViewModelBase : INotifyPropertyChanged
{
    ChannelAppearance Appearance { get; }
    string DisplayName { get; }
    string ShortName { get; }

    bool HasLeftMeter { get; }
    bool HasRightMeter { get; }
    bool HasMute { get; }
    // TODO: Potentially change Output/StereoOutput to LeftMeterLevel and RightMeterLevel, ditto peaks.
    MeterLevel Output { get; }
    MeterLevel StereoOutput { get; }
    MeterLevel PeakOutput { get; }
    MeterLevel StereoPeakOutput { get; }
    bool Muted { get; set; }
    IReadOnlyList<FaderViewModel> Faders { get; }
}
