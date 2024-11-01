using DigiMixer.AppCore;
using JonSkeet.CoreAppUtil;

namespace DigiMixer.Wpf;

public class ChannelMappingViewModel : ViewModelBase<ChannelMapping>
{
    public ChannelMappingViewModel(ChannelMapping model) : base(model)
    {
    }

    public string EffectiveDisplayName => Model.EffectiveDisplayName;

    [RelatedProperties(nameof(EffectiveDisplayName))]
    public string Name
    {
        get => Model.DisplayName;
        set => SetProperty(Name, value, x => Model.DisplayName = x);
    }

    public int Number
    {
        get => Model.Channel;
        set => SetProperty(Model.Channel, value, x => Model.Channel = x);
    }
}
