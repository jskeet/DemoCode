using DigiMixer.AppCore;
using JonSkeet.CoreAppUtil;
using System.Windows.Input;

namespace DigiMixer.Wpf;

public class ChannelListViewModel : ViewModelBase
{
    private readonly string idPrefix;
    public SelectableCollection<ChannelMappingViewModel> Mappings { get; } = [];

    public ICommand AddChannelCommand { get; }

    public ChannelListViewModel(string idPrefix)
    {
        AddChannelCommand = ActionCommand.FromAction(AddChannel);
        this.idPrefix = idPrefix;
    }

    private void AddChannel()
    {
        // Generate an ID that's unused and somewhat plausible.
        int index = Mappings.Count + 1;
        while (Mappings.Any(m => m.Model.Id == $"{idPrefix}{index}"))
        {
            index++;
        }
        string id = $"{idPrefix}{index}";
        // Find the first unused channel number
        int channel = 1;
        while (Mappings.Any(m => m.Number == channel))
        {
            channel++;
        }
        var mapping = new ChannelMappingViewModel(new ChannelMapping
        {
            Id = $"{idPrefix}{index}",
            DisplayName = "Unnamed channel",
            Channel = channel,
            InitiallyVisible = true
        });
        Mappings.AddAndSelect(mapping);
    }
}
