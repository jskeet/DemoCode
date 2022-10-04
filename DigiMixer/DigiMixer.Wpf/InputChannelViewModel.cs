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
            Brushes.Khaki
        };

    public IReadOnlyList<FaderViewModel> Faders { get; set; }

    public InputChannelViewModel(InputChannel model) : base(model, "id", null)
    {
        Faders = model.OutputMappings
            .Select((mapping, index) => new FaderViewModel(mapping, faderBackgrounds[index]))
            .ToList()
            .AsReadOnly();
    }
}
