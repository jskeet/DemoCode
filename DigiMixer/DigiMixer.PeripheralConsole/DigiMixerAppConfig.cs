using DigiMixer.Controls;

// Copied from DigiMixer.Wpf so the same config can be used in both apps.
namespace DigiMixer.PeripheralConsole;

public class DigiMixerAppConfig
{
    public LoggingConfig Logging { get; set; } = new();
    public DigiMixerConfig Mixer { get; set; } = new();
    public bool EnablePeripherals { get; set; }
}
