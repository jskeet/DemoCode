using System.ComponentModel;
using System.Windows.Media;

namespace DigiMixer.Wpf;

public class FaderViewModel : ViewModelBase<IFader>
{
    // For binding purposes.
    public static double FaderLevelScaleDouble { get; } = DigiMixer.FaderLevel.MaxValue;

    internal FaderViewModel(IFader model, Brush background) : base(model)
    {
        Background = background;
    }

    public Brush Background { get; }

    public int FaderLevel
    {
        get => Model.FaderLevel.Value;
        set => Model.SetFaderLevel(new FaderLevel(value));
    }

    /// <summary>
    /// The level of the fader, as a percentage.
    /// </summary>
    public string FaderLevelPercentage => $"{FaderLevel * 100 / DigiMixer.FaderLevel.MaxValue}%";

    protected override void OnPropertyModelChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IFader.FaderLevel))
        {
            RaisePropertyChanged(nameof(FaderLevel));
            RaisePropertyChanged(nameof(FaderLevelPercentage));
        }
    }
}
