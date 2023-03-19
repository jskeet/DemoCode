using DigiMixer.Core;

namespace DigiMixer;

public interface IFader
{
    FaderLevel FaderLevel { get; }
    void SetFaderLevel(FaderLevel level);
}
