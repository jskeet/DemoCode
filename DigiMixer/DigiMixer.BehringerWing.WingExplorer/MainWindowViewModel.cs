using DigiMixer.BehringerWing.Core;
using JonSkeet.CoreAppUtil;
using JonSkeet.WpfUtil;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;

namespace DigiMixer.BehringerWing.WingExplorer;

public class MainWindowViewModel : ViewModelBase
{
    private object? currentVm;

    public MixerDetailsViewModel MixerDetails { get; }
    public ProgressViewModel Progress { get; }
    public ExplorerViewModel Explorer { get; }

    public bool MixerDetailsVisible => currentVm is MixerDetailsViewModel;
    public bool ProgressVisible => currentVm is ProgressViewModel;
    public bool ExplorerVisible => currentVm is ExplorerViewModel;

    private bool dataLoaded;
    public bool DataLoaded
    {
        get => dataLoaded;
        set => SetProperty(ref dataLoaded, value);
    }

    public MainWindowViewModel()
    {
        MixerDetails = new() { ConnectCommand = ActionCommand.FromAction(Connect) };
        Progress = new() { DisplayCommand = ActionCommand.FromAction(DisplayExplorer).WithCanExecuteProperty(this, nameof(DataLoaded)) };
        Explorer = new();
        SetViewModel(MixerDetails);
    }

    internal void SetViewModel(object vm)
    {
        currentVm = vm;
        RaisePropertyChanged(nameof(MixerDetailsVisible));
        RaisePropertyChanged(nameof(ProgressVisible));
        RaisePropertyChanged(nameof(ExplorerVisible));
    }

    private async Task Connect()
    {
        SetViewModel(Progress);
        try
        {
            Explorer.SetRoot(await LoadData(MixerDetails.IPAddress, MixerDetails.Port));
        }
        catch (Exception e)
        {
            Dialogs.ShowErrorDialog("Error loading data", e.Message);
            // The user will basically just have to close the dialog at this point.
            return;
        }

        DataLoaded = true;
    }

    private void DisplayExplorer()
    {
        SetViewModel(Explorer);
    }

    private async Task<MixerTreeNode> LoadData(string address, ushort port)
    {
        uint? currentNodeHash = null;

        ConcurrentDictionary<uint, WingNodeDefinition> definitions = new();
        ConcurrentDictionary<uint, WingToken> values = new();

        bool loadingDefinitions = true;
        var tcs = new TaskCompletionSource();
        Stack<uint> pendingDefinitionRequests = new();
        var client = new WingClient(NullLogger<WingClient>.Instance, address, port);
        client.AudioEngineTokenReceived += HandleToken;
        await client.Connect(default);
        client.Start();

        await client.SendAudioEngineTokens([WingToken.RootNode, WingToken.DefinitionRequest], default);
        await tcs.Task;
        client.Dispose();
        return CreateTree(definitions, values);

        void HandleToken(object? sender, WingToken token)
        {
            switch (token.Type)
            {
                case WingTokenType.NodeHash:
                    currentNodeHash = token.NodeHash;
                    break;
                case WingTokenType.NodeDefinition:
                    WingNodeDefinition nodeDefinition = token.NodeDefinition;
                    if (definitions.TryAdd(nodeDefinition.NodeHash, nodeDefinition))
                    {
                        if (nodeDefinition.Flags == 0)
                        {
                            pendingDefinitionRequests.Push(nodeDefinition.NodeHash);
                            Progress.PendingNodeDefinitionCount = pendingDefinitionRequests.Count;
                        }
                        Progress.NodeDefinitionCount++;
                    }
                    break;
                case WingTokenType.FalseOffZero:
                case WingTokenType.TrueOnOne:
                case WingTokenType.Int16:
                case WingTokenType.String:
                case WingTokenType.Int32:
                case WingTokenType.Float32:
                case WingTokenType.RawFloat32:
                case WingTokenType.Step:
                case WingTokenType.Toggle:
                    if (currentNodeHash is uint nodeHash)
                    {
                        if (values.TryAdd(nodeHash, token))
                        {
                            Progress.NodeDataCount++;
                        }
                    }
                    currentNodeHash = null;
                    break;
                case WingTokenType.EndOfRequest:
                    if (loadingDefinitions)
                    {
                        if (pendingDefinitionRequests.TryPop(out uint nextHash))
                        {
                            // TODO: Make all of this properly asynchronous. This is horrible at the moment.
                            client.SendAudioEngineTokens([WingToken.ForNodeHash(nextHash), WingToken.DefinitionRequest], default);
                            Progress.PendingNodeDefinitionCount = pendingDefinitionRequests.Count;
                        }
                        else
                        {
                            loadingDefinitions = false;
                            client.SendAudioEngineTokens([WingToken.RootNode, WingToken.DataRequest], default).Ignore();
                        }
                    }
                    else
                    {
                        DataLoaded = true;
                        tcs.SetResult();
                    }
                    break;
            }
        }
    }

    public MixerTreeNode CreateTree(IReadOnlyDictionary<uint, WingNodeDefinition> definitions, IReadOnlyDictionary<uint, WingToken> values)
    {
        var names = new Dictionary<uint, string>();
        var fullNames = new Dictionary<uint, string>();
        var fields = definitions.Where(pair => !pair.Value.IsNode);
        var nodes = definitions.Where(pair => pair.Value.IsNode).ToDictionary();

        nodes[0] = WingNodeDefinition.Root;
        names[0] = "Root";
        fullNames[0] = "Root";
        foreach (var definition in definitions.Values)
        {
            names[definition.NodeHash] = EmptyToNull(definition.Name) ?? EmptyToNull(definition.LongName) ?? definition.NodeIndex.ToString();
        }

        foreach (var definition in definitions.Values)
        {
            PopulateFullName(definition);
        }

        var treeNodes = nodes.ToDictionary(pair => pair.Key, pair => new MixerTreeNode(pair.Value, fullNames[pair.Key], names[pair.Key]));
        var treeFields = definitions.Where(pair => !pair.Value.IsNode).Select(pair => new MixerTreeField(pair.Value, values.GetValueOrDefault(pair.Key), fullNames[pair.Key], names[pair.Key])).ToList();

        // Add nodes to their parents
        foreach (var pair in treeNodes)
        {
            var definition = pair.Value.Definition;
            if (definition.NodeHash != 0)
            {
                var parent = treeNodes[definition.ParentHash];
                parent.Children.Add(pair.Value);
            }
        }
        // Add fields to their parents
        foreach (var field in treeFields)
        {
            treeNodes[field.Definition.ParentHash].Fields.Add(field);
        }

        foreach (var node in treeNodes.Values)
        {
            node.SortAllChildren();
        }

        // Done. We only need to return the root - everything else should now be in place.
        return treeNodes[0];

        string PopulateFullName(WingNodeDefinition definition)
        {
            if (fullNames.TryGetValue(definition.NodeHash, out var existing))
            {
                return existing;
            }
            string name = names[definition.NodeHash];
            string fullName = definition.ParentHash == 0 ? name : $"{PopulateFullName(nodes[definition.ParentHash])}.{name}";
            fullNames[definition.NodeHash] = fullName;
            return fullName;
        }

        string? EmptyToNull(string text) => text == "" ? null : text;
    }
}
