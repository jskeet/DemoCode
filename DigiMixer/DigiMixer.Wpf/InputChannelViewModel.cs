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

    public InputChannelViewModel(InputChannel input, IReadOnlyDictionary<ChannelId, OutputChannel> channelIdToOutputChannel) : base(input, "id", null)
    {
        Faders = input.OutputMappings
            // TODO: Avoid needing this dictionary. Ideally the Mixer should expose output mappings in which have the channels, not just channel IDs.
            .Select((mapping, index) => new FaderViewModel(mapping, channelIdToOutputChannel[mapping.PrimaryOutputChannelId], faderBackgrounds[index]))
            .ToList()
            .AsReadOnly();
    }
}
