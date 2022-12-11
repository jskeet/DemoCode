using DigiMixer.Core;
using System.ComponentModel;
using System.Windows.Threading;

namespace DigiMixer.Wpf;

public class MixerViewModel : ViewModelBase<Mixer>, IDisposable
{
    public IReadOnlyList<InputChannelViewModel> InputChannels { get; }
    public IReadOnlyList<OutputChannelViewModel> OutputChannels { get; }
    private DispatcherTimer meterPeakUpdater;

    public MixerViewModel(Mixer model) : base(model)
    {
        // FIXME: Need IDs and display names.
        InputChannels = Model.InputChannels
            .Select(input => new InputChannelViewModel(input))
            .ToList()
            .AsReadOnly();
        OutputChannels = Model.OutputChannels
            .Select(output => new OutputChannelViewModel(output))
            .ToList()
            .AsReadOnly();
        meterPeakUpdater = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100),
            IsEnabled = true
        };
        meterPeakUpdater.Tick += UpdateMeterPeaks;
    }

    private void UpdateMeterPeaks(object sender, EventArgs e)
    {
        foreach (var vm in InputChannels)
        {
            vm.UpdatePeakOutputs();
        }
        foreach (var vm in OutputChannels)
        {
            vm.UpdatePeakOutputs();
        }
    }

    public MixerInfo MixerInfo => Model.MixerInfo ?? new MixerInfo("", "", "");

    protected override void OnPropertyModelChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model.MixerInfo))
        {
            RaisePropertyChanged(nameof(MixerInfo));
        }
    }

    public void Dispose()
    {
        meterPeakUpdater.Stop();
        Model.Dispose();
    }
}
