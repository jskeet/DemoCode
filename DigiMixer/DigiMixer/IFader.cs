using DigiMixer.Core;
using System.ComponentModel;

namespace DigiMixer;

public interface IFader : INotifyPropertyChanged
{
    FaderLevel FaderLevel { get; }
    void SetFaderLevel(FaderLevel level);

    /// <summary>
    /// The scale of the fader, which is always the same as the scale for the overall mixer.
    /// </summary>
    IFaderScale Scale { get; }
}
