using DigiMixer.Core;
using DigiMixer.Wpf.Utilities;
using System.ComponentModel;
using System.Windows.Media;

namespace DigiMixer.Wpf;

public class FaderViewModel : ViewModelBase<IFader>
{
    // For binding purposes.
    public static double FaderLevelScaleDouble { get; } = Core.FaderLevel.MaxValue;

    private readonly OutputChannel outputChannel;

    internal FaderViewModel(IFader model, OutputChannel outputChannel, Brush background) : base(model)
    {
        Background = background;
        this.outputChannel = outputChannel;
    }

    // We only consider the output channel name, as this is either a fader on an input channel control
    // which will be entirely hidden if the input is unnamed, or a fader on an output channel control
    // in which case it'll already be hidden, but it doesn't matter if we hide it again :)
    public bool Visible => outputChannel.Name is not null;

    protected override void OnPropertyChangedHasSubscribers()
    {
        base.OnPropertyChangedHasSubscribers();
        outputChannel.PropertyChanged += HandleOutputChannelPropertyChange;
    }

    protected override void OnPropertyChangedHasNoSubscribers()
    {
        base.OnPropertyChangedHasNoSubscribers();
        outputChannel.PropertyChanged -= HandleOutputChannelPropertyChange;
    }

    private void HandleOutputChannelPropertyChange(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(outputChannel.Name))
        {
            RaisePropertyChanged(nameof(Visible));
        }
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
    public string FaderLevelPercentage => $"{FaderLevel * 100 / Core.FaderLevel.MaxValue}%";

    protected override void OnPropertyModelChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IFader.FaderLevel))
        {
            RaisePropertyChanged(nameof(FaderLevel));
            RaisePropertyChanged(nameof(FaderLevelPercentage));
        }
    }
}
