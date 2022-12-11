using DigiMixer.Core;

namespace DigiMixer;

public interface IFader
{
    FaderLevel FaderLevel { get; }
    Task SetFaderLevel(FaderLevel level);
}
