using JonSkeet.WpfUtil;
using System.Collections.ObjectModel;

namespace DigiMixer.Wpf;

public class ChannelListViewModel : ViewModelBase, IReorderableList
{
    public ObservableCollection<ChannelMappingViewModel> Mappings { get; } = new();

    private ChannelMappingViewModel selectedMapping;
    public ChannelMappingViewModel SelectedMapping
    {
        get => selectedMapping;
        set => SetProperty(ref selectedMapping, value);
    }

    public void DeleteSelectedItem() => SelectedMapping = Mappings.RemoveSelected(SelectedMapping);
    public void MoveSelectedItemUp() => Mappings.MoveSelectedItemUp(SelectedMapping, value => SelectedMapping = value);
    public void MoveSelectedItemDown() => Mappings.MoveSelectedItemDown(SelectedMapping, value => SelectedMapping = value);
}
