using DigiMixer.Controls;
using JonSkeet.WpfLogging;

namespace DigiMixer.Wpf;

public class DigiMixerAppConfig
{
    public LoggingConfig Logging { get; set; } = new();
    public DigiMixerConfig Mixer { get; set; } = new();
    public bool EnablePeripherals { get; set; }
}
