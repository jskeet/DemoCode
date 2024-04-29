using DigiMixer.Controls;
using JonSkeet.CoreAppUtil;
using JonSkeet.WpfUtil;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DigiMixer.Wpf;

public class ChannelListViewModel : ViewModelBase, IReorderableList
{
    private readonly 
        string idPrefix;
    public ObservableCollection<ChannelMappingViewModel> Mappings { get; } = new();

    public ICommand AddChannelCommand { get; }

    private ChannelMappingViewModel selectedMapping;
    public ChannelMappingViewModel SelectedMapping
    {
        get => selectedMapping;
        set => SetProperty(ref selectedMapping, value);
    }

    public ChannelListViewModel(string idPrefix)
    {
        AddChannelCommand = ActionCommand.FromAction(AddChannel);
        this.idPrefix = idPrefix;
    }

    public void DeleteSelectedItem() => SelectedMapping = Mappings.RemoveSelected(SelectedMapping);
    public void MoveSelectedItemUp() => Mappings.MoveSelectedItemUp(SelectedMapping, value => SelectedMapping = value);
    public void MoveSelectedItemDown() => Mappings.MoveSelectedItemDown(SelectedMapping, value => SelectedMapping = value);

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
        Mappings.Add(mapping);
        SelectedMapping = mapping;
    }
}
