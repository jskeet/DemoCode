using System.Windows.Media;

namespace DigiMixer.Wpf;

public class InputChannelViewModel : ChannelViewModelBase<InputChannel>
{
    private static readonly Brush[] faderBackgrounds =
    {
        Brushes.PowderBlue,
        Brushes.PaleGoldenrod,
        Brushes.Lavender,
        Brushes.LightPink,
        Brushes.DarkSeaGreen,
        Brushes.Khaki,
        Brushes.DarkCyan,
        Brushes.Aquamarine,
        Brushes.Chartreuse,
        Brushes.Crimson,
        Brushes.DarkMagenta,
        Brushes.LightGray,
        Brushes.LightSteelBlue,
        Brushes.Salmon,
        Brushes.MediumSpringGreen,
        Brushes.MidnightBlue,
        Brushes.MintCream,
        Brushes.MistyRose
    };

    public IReadOnlyList<FaderViewModel> Faders { get; set; }

    public InputChannelViewModel(InputChannel input) : base(input, "id", null)
    {
        Faders = input.OutputMappings
            .Select((mapping, index) => new FaderViewModel(mapping, mapping.OutputChannel, faderBackgrounds[index]))
            .ToList()
            .AsReadOnly();
    }
}
