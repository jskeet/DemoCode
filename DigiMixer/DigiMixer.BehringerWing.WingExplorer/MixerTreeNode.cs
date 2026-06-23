using DigiMixer.BehringerWing.Core;

namespace DigiMixer.BehringerWing.WingExplorer;

public class MixerTreeNode(WingNodeDefinition definition, string fullName, string name)
{
    public WingNodeDefinition Definition => definition;
    public string FullName => fullName;
    public string Name => name;
    public uint Hash => Definition.NodeHash;

    public List<MixerTreeNode> Children { get; private set; } = [];
    public List<MixerTreeField> Fields { get; private set; } = [];

    // TODO: Maybe use a builder pattern instead of mutating.
    public void SortAllChildren()
    {
        Children = [.. Children
            .OrderBy(node => node.Definition.NodeIndex)
            // This handles things like "ch.1.send" where the children "1", "2", "3" etc don't have node indexes - but sorting by name gives "1", "10", "11", ..., "2"
            .ThenBy(node => int.TryParse(node.Name, out int value) ? value : int.MaxValue)
            .ThenBy(node => node.Definition.LongName)
            .ThenBy(node => node.Definition.Name)];
        Fields = [.. Fields.OrderBy(node => node.Definition.NodeIndex)
            .ThenBy(node => int.TryParse(node.Name, out int value) ? value : int.MaxValue)
            .ThenBy(node => node.Definition.LongName)
            .ThenBy(node => node.Definition.Name)];
    }

    public override string ToString() => FullName;
}
