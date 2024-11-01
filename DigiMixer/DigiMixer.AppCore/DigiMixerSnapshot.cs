namespace DigiMixer.AppCore;

/// <summary>
/// Snapshot of mixer status, with respect to configuration mappings.
/// </summary>
public class DigiMixerSnapshot
{
    public Dictionary<string, MixerInputChannelSnapshot> InputChannels { get; set; }
    public Dictionary<string, MixerOutputChannelSnapshot> OutputChannels { get; set; }

    /// <summary>
    /// Creates a snapshot from the given MixerViewModel, or returns null if
    /// not all channels have received data yet.
    /// </summary>
    public static DigiMixerSnapshot FromMixerViewModel(DigiMixerViewModel vm) =>
        new DigiMixerSnapshot
        {
            InputChannels = vm.InputChannels.ToDictionary(vm => vm.Id, MixerInputChannelSnapshot.FromChannelViewModel),
            OutputChannels = vm.OutputChannels.ToDictionary(vm => vm.Id, MixerOutputChannelSnapshot.FromChannelViewModel)
        };

    public void CopyToViewModel(DigiMixerViewModel vm)
    {
        foreach (var input in vm.InputChannels)
        {
            if (InputChannels.TryGetValue(input.Id, out var snapshot))
            {
                snapshot.CopyToViewModel(input);
            }
        }
        foreach (var output in vm.OutputChannels)
        {
            if (OutputChannels.TryGetValue(output.Id, out var snapshot))
            {
                snapshot.CopyToViewModel(output);
            }
        }
    }
}

public class MixerInputChannelSnapshot
{
    public bool Muted { get; set; }
    public Dictionary<string, int> FaderLevels { get; set; }

    public static MixerInputChannelSnapshot FromChannelViewModel(InputChannelViewModel vm) =>
        new MixerInputChannelSnapshot
        {
            Muted = vm.Muted,
            FaderLevels = vm.Faders.ToDictionary(f => f.OutputId, f => f.FaderLevel)
        };

    public void CopyToViewModel(InputChannelViewModel vm)
    {
        vm.Muted = Muted;
        foreach (var fader in vm.Faders)
        {
            if (FaderLevels.TryGetValue(fader.OutputId, out var level))
            {
                fader.FaderLevel = level;
            }
        }
    }
}

public class MixerOutputChannelSnapshot
{
    public bool? Muted { get; set; }
    public int FaderLevel { get; set; }

    public static MixerOutputChannelSnapshot FromChannelViewModel(OutputChannelViewModel vm) => new MixerOutputChannelSnapshot
    {
        FaderLevel = vm.OverallFader.FaderLevel,
        Muted = vm.Muted
    };

    public void CopyToViewModel(OutputChannelViewModel vm)
    {
        vm.OverallFader.FaderLevel = FaderLevel;
        if (Muted is bool muted)
        {
            vm.Muted = muted;
        }
    }
}
