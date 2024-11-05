using JonSkeet.CoreAppUtil;
using System.Windows.Input;

namespace DigiMixer.BehringerWing.WingExplorer;

public class ProgressViewModel : ViewModelBase
{
    private int nodeDefinitionCount;
    public int NodeDefinitionCount
    {
        get => nodeDefinitionCount;
        set => SetProperty(ref nodeDefinitionCount, value);
    }

    private int pendingNodeDefinitionCount;
    public int PendingNodeDefinitionCount
    {
        get => pendingNodeDefinitionCount;
        set => SetProperty(ref pendingNodeDefinitionCount, value);
    }

    private int nodeDataCount;
    public int NodeDataCount
    {
        get => nodeDataCount;
        set => SetProperty(ref nodeDataCount, value);
    }

    public required ICommand DisplayCommand { get; init; }
}
