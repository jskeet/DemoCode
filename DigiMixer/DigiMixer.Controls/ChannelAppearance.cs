using JonSkeet.WpfUtil;
using System.Windows.Media;
using static System.Windows.Media.Brushes;

namespace DigiMixer.Controls;

/// <summary>
/// The overall appearance of an input or output channel.
/// </summary>
public class ChannelAppearance : ViewModelBase
{
    private static BrushConverter brushConverter = new BrushConverter();
    private static readonly Brush[] defaultBrushes =
    {
        AntiqueWhite,
        Beige,
        BurlyWood,
        CadetBlue,
        Gold,
        Lavender,
        Khaki,
        LavenderBlush,
        LemonChiffon,
        LightBlue,
        LightCyan,
        LightGreen,
        LightPink,
        LightSalmon,
        LightSkyBlue,
        LightSteelBlue,
        MediumAquamarine,
        MediumTurquoise,
        MistyRose,
        Moccasin,
        Orchid,
        PaleTurquoise,
        PapayaWhip,
        PeachPuff,
        Pink,
        Plum,
        Silver,
        SkyBlue,
        Violet,
        YellowGreen
    };

    public Brush Background { get; }

    private bool visible;
    public bool Visible
    {
        get => visible;
        set => SetProperty(ref visible, value);
    }

    internal static ChannelAppearance ForMapping(ChannelMapping mapping)
    {
        var brush = !string.IsNullOrEmpty(mapping.Color) ? (Brush) brushConverter.ConvertFrom(mapping.Color)
            : mapping.Channel == 100 ? White
            : defaultBrushes[mapping.Channel % defaultBrushes.Length];
        return new ChannelAppearance(brush, mapping.InitiallyVisible);
    }

    internal static ChannelAppearance CreateVisibleWhite() => new ChannelAppearance(White, true);

    private ChannelAppearance(Brush background, bool visible)
    {
        Background = background;
        Visible = visible;
    }
}
