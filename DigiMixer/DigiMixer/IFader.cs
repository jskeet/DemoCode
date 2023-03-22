using DigiMixer.Core;
using System.ComponentModel;

namespace DigiMixer;

public interface IFader : INotifyPropertyChanged
{
    FaderLevel FaderLevel { get; }
    void SetFaderLevel(FaderLevel level);
}
