using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DigiMixer.Wpf;

public class MixerViewModel : ViewModelBase<Mixer>
{
    public IReadOnlyList<InputChannelViewModel> InputChannels { get; }
    public IReadOnlyList<OutputChannelViewModel> OutputChannels { get; }

    public MixerViewModel(Mixer model) : base(model)
    {
        // FIXME: Need IDs and display names.
        InputChannels = Model.InputChannels
            .Select(channel => new InputChannelViewModel(channel))
            .ToList()
            .AsReadOnly();
        OutputChannels = Model.OutputChannels
            .Select(channel => new OutputChannelViewModel(channel))
            .ToList()
            .AsReadOnly();
    }

    public MixerInfo MixerInfo => Model.MixerInfo ?? new MixerInfo("", "", "");

    protected override void OnPropertyModelChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model.MixerInfo))
        {
            RaisePropertyChanged(nameof(MixerInfo));
        }
    }
}
