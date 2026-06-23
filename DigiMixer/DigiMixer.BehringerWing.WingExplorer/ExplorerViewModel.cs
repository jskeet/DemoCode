using JonSkeet.CoreAppUtil;
using System.Collections.ObjectModel;

namespace DigiMixer.BehringerWing.WingExplorer;

public class ExplorerViewModel : ViewModelBase
{
    public ObservableCollection<MixerTreeNode> RootNodes { get; } = [];

    public void SetRoot(MixerTreeNode node)
    {
        RootNodes.Clear();
        RootNodes.Add(node);
    }
}
