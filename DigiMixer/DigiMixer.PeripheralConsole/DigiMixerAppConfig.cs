
// Copied from DigiMixer.Wpf (and trimmed) so the same config can be used in both apps.
using DigiMixer.AppCore;

namespace DigiMixer.PeripheralConsole;

public class DigiMixerAppConfig
{
    public LoggingConfig Logging { get; set; } = new();
    public DigiMixerConfig Mixer { get; set; } = new();
}
