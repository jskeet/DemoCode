using JonSkeet.CoreAppUtil;

namespace DigiMixer.Controls;

/// <summary>
/// The overall appearance of an input or output channel.
/// </summary>
public class ChannelAppearance : ViewModelBase
{
    private const string White = "White";

    private static readonly string[] defaultBrushes =
    {
        "AntiqueWhite",
        "Beige",
        "BurlyWood",
        "CadetBlue",
        "Gold",
        "Lavender",
        "Khaki",
        "LavenderBlush",
        "LemonChiffon",
        "LightBlue",
        "LightCyan",
        "LightGreen",
        "LightPink",
        "LightSalmon",
        "LightSkyBlue",
        "LightSteelBlue",
        "MediumAquamarine",
        "MediumTurquoise",
        "MistyRose",
        "Moccasin",
        "Orchid",
        "PaleTurquoise",
        "PapayaWhip",
        "PeachPuff",
        "Pink",
        "Plum",
        "Silver",
        "SkyBlue",
        "Violet",
        "YellowGreen"
    };

    public string Background { get; }

    private bool visible;
    public bool Visible
    {
        get => visible;
        set => SetProperty(ref visible, value);
    }

    internal static ChannelAppearance ForMapping(ChannelMapping mapping)
    {
        var brush = !string.IsNullOrEmpty(mapping.Color) ? mapping.Color
            : mapping.Channel == 100 ? White
            : defaultBrushes[mapping.Channel % defaultBrushes.Length];
        return new ChannelAppearance(brush, mapping.InitiallyVisible);
    }

    internal static ChannelAppearance CreateVisibleWhite() => new ChannelAppearance(White, true);

    private ChannelAppearance(string background, bool visible)
    {
        Background = background;
        Visible = visible;
    }
}
