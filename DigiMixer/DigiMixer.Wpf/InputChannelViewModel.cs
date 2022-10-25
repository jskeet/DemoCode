using System.Collections.Generic;
using System.Linq;
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
        Brushes.MediumSpringGreen
    };

    public IReadOnlyList<FaderViewModel> Faders { get; set; }

    public InputChannelViewModel(InputChannel input) : base(input, "id", null)
    {
        Faders = input.OutputMappings
            // TODO: handle stereo pairs properly. (This code is not only ghastly, but it's broken - we're ignoring the flags...)
            .Select((mapping, index) => new FaderViewModel(mapping, faderBackgrounds[index]))
            .ToList()
            .AsReadOnly();
    }
}
