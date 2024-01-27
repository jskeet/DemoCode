using JonSkeet.WpfUtil;
using DigiMixer;
using Microsoft.Extensions.Logging;
using NodaTime;
using System.Windows.Threading;
using System.Windows.Input;
using System.IO;
using NodaTime.Text;

namespace DigiMixer.Controls;

/// <summary>
/// The view model for the mixer, which may initial not be connected.
/// Once the mixer has been connected once, the configuration is assumed to
/// be static, even if it disconnects and reconnects.
/// </summary>
public class DigiMixerViewModel : ViewModelBase, IDisposable
{
    private static readonly InstantPattern DefaultSnapshotFilePattern = InstantPattern.CreateWithInvariantCulture("'Snapshot ' uuuu-MM-dd HH-mm'Z.json'");

    /// <summary>
    /// Input channels with associated output faders.
    /// </summary>
    public IReadOnlyList<InputChannelViewModel> InputChannels { get; }

    /// <summary>
    /// Output channels with all associated faders (input channels and overall)
    /// </summary>
    public IReadOnlyList<OutputChannelViewModel> OutputChannels { get; }

    /// <summary>
    /// Output channels with a single fader each (just the overall output).
    /// </summary>
    public IReadOnlyList<OutputChannelViewModel> OverallOutputChannels { get; }

    /// <summary>
    /// Input channels with no associated faders.
    /// </summary>
    public IReadOnlyList<InputChannelViewModel> InputsWithNoFaders { get; }

    /// <summary>
    /// A channel group of inputs, with a fader per visible output.
    /// This is shown as in the "group by input" configuration.
    /// </summary>
    public ChannelGroupViewModel InputsGroup { get; }

    /// <summary>
    /// A channel group of outputs, with a main fader and a fader per visible input.
    /// This is shown as in the "group by output" configuration.
    /// </summary>
    public ChannelGroupViewModel OutputsWithInputsGroup { get; }

    /// <summary>
    /// A channel group of just outputs with main faders.
    /// This is shown as in the "group by input" configuration.
    /// </summary>
    public ChannelGroupViewModel OverallOutputsGroup { get; }

    /// <summary>
    /// A channel group of just inputs, with no faders (so just mute buttons and meters).
    /// This is shown as in the "group by output" configuration.
    /// </summary>
    public ChannelGroupViewModel InputsMuteAndMeterGroup { get; }

    private bool groupByInput = true;
    /// <summary>
    /// Whether the channels should be grouped by input (i.e. one "channel strip" per input,
    /// showing all the output faders for that input, with another strip for all the "overall
    /// outputs" below) or grouped by output (i.e. one channel strip per output, with all
    /// the inputs and the overall output at the start).
    /// </summary>
    [RelatedProperties(nameof(GroupByOutput))]
    public bool GroupByInput
    {
        get => groupByInput;
        set
        {
            if (SetProperty(ref groupByInput, value))
            {
                InputsGroup.Visible = value;
                OverallOutputsGroup.Visible = value;
                OutputsWithInputsGroup.Visible = !value;
                InputsMuteAndMeterGroup.Visible = !value;
            }
        }
    }

    // The inverse of GroupByInput, just for simple binding.
    public bool GroupByOutput
    {
        get => !GroupByInput;
        set => GroupByInput = !value;
    }

    public ICommand MuteAllCommand { get; }
    public ICommand LoadSnapshotCommand { get; }
    public ICommand SaveSnapshotCommand { get; }

    private readonly ILogger logger;
    private readonly string snapshotsDirectory;

    internal DigiMixerConfig Config { get; }
    private Mixer mixer;
    private bool disposed;
    private DispatcherTimer meterPeakUpdater;

    public StatusViewModel Status { get; } = new StatusViewModel("Mixer");
    public int MaxFaderLevelValue => mixer?.FaderScale.MaxValue ?? 100_000;

    public DigiMixerViewModel(ILogger logger, DigiMixerConfig config, string snapshotsFolder)
    {
        Config = config;
        this.logger = logger;
        this.snapshotsDirectory = snapshotsFolder;
        Status.ReportNormal("Connecting");
        MuteAllCommand = ActionCommand.FromAction(MuteAll);
        LoadSnapshotCommand = ActionCommand.FromAction(LoadSnapshot);
        SaveSnapshotCommand = ActionCommand.FromAction(SaveSnapshot);

        // We build up the input/output channels in two passes, as they need to refer to each other.
        Duration feedbackMutingDuration = Duration.FromMilliseconds(config.FeedbackMutingMillis);
        InputChannels = config.InputChannels.ToReadOnlyList(mapping => new InputChannelViewModel(mapping, config.FeedbackMutingThreshold, feedbackMutingDuration));
        OutputChannels = config.OutputChannels.ToReadOnlyList(mapping => new OutputChannelViewModel(mapping));
        OverallOutputChannels = config.OutputChannels.ToReadOnlyList(mapping => new OutputChannelViewModel(mapping));
        InputsWithNoFaders = config.InputChannels.ToReadOnlyList(mapping => new InputChannelViewModel(mapping, null, null));

        InputsGroup = new ChannelGroupViewModel("Inputs", InputChannels, true);
        OutputsWithInputsGroup = new ChannelGroupViewModel("Outputs", OutputChannels, false);
        OverallOutputsGroup = new ChannelGroupViewModel("Outputs", OverallOutputChannels, true);
        InputsMuteAndMeterGroup = new ChannelGroupViewModel("Inputs", InputsWithNoFaders, false);
        foreach (var inputChannel in InputChannels)
        {
            inputChannel.SetFaders(OutputChannels);
        }
        foreach (var outputChannel in OutputChannels)
        {
            outputChannel.SetFaders(InputChannels);
        }
        foreach (var outputChannel in OverallOutputChannels)
        {
            outputChannel.SetFaders(Enumerable.Empty<InputChannelViewModel>());
        }
        // No SetFaders call for InputsWithNoFaders

        Task mixerCreationTask = CreateMixer();
        mixerCreationTask.ContinueWith(task => logger.LogError("Mixer creation task failed"), TaskContinuationOptions.NotOnRanToCompletion);

        async Task CreateMixer()
        {
            while (!disposed)
            {
                try
                {
                    Mixer mixer = await Mixer.Create(logger, () => config.CreateMixerApi(logger));
                    SetMixer(mixer);
                    logger.LogInformation("Initial connection to mixer successful");
                    return;
                }
                catch (Exception e)
                {
                    Status.ReportError("Failed to connect");
                    logger.LogError(e, "Error creating mixer... retrying in a few seconds.");
                    await Task.Delay(3000);
                }
            }
        }
    }

    // This happens once, when we've managed to connect for the first time,
    // at which point we assume all the channel configuration has been determined.
    private void SetMixer(Mixer mixer)
    {
        UpdateMixerStatus();
        this.mixer = mixer;
        mixer.PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(Mixer.MixerInfo):
                case nameof(Mixer.Connected):
                    UpdateMixerStatus();
                    break;
            }
        };
        foreach (var input in InputChannels.Concat(InputsWithNoFaders))
        {
            input.UpdateFromMixer(mixer);
        }
        foreach (var output in OutputChannels.Concat(OverallOutputChannels))
        {
            output.UpdateFromMixer(mixer);
        }
        meterPeakUpdater = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100),
            IsEnabled = true
        };
        meterPeakUpdater.Tick += UpdateMeterPeaks;

        void UpdateMixerStatus()
        {
            if (mixer.Connected)
            {
                Status.ReportNormal(mixer.MixerInfo?.ToString() ?? "Connected");
            }
            else
            {
                Status.ReportError("Disconnected");
            }
        }
    }

    public void Dispose()
    {
        disposed = true;
        meterPeakUpdater?.Stop();
        mixer?.Dispose();
    }

    private void UpdateMeterPeaks(object sender, EventArgs e)
    {
        foreach (var vm in InputChannels)
        {
            vm.UpdatePeakOutputs(logger);
        }
        foreach (var vm in OutputChannels)
        {
            vm.UpdatePeakOutputs(logger);
        }
        foreach (var vm in OverallOutputChannels)
        {
            vm.UpdatePeakOutputs(logger);
        }
        foreach (var vm in InputsWithNoFaders)
        {
            vm.UpdatePeakOutputs(logger);
        }
    }


    private void MuteAll()
    {
        foreach (var channelVm in InputChannels)
        {
            channelVm.Muted = true;
        }
    }

    private void SaveSnapshot()
    {
        var snapshot = DigiMixerSnapshot.FromMixerViewModel(this);
        if (snapshot is null)
        {
            // This isn't really great, but it's unlikely to be a problem.
            Dialogs.ShowErrorDialog("Waiting for data", "The mixer has not received all the necessary data yet. Please wait and try again.");
            return;
        }
        MaybeCreateSnapshotsDirectory();
        var now = SystemClock.Instance.GetCurrentInstant();
        var defaultFile = DefaultSnapshotFilePattern.Format(now);
        var file = Dialogs.ShowSaveFileDialog(snapshotsDirectory, "JSON files|*.json", defaultFile);
        if (file is null)
        {
            return;
        }
        JsonUtilities.SaveJson(file, snapshot);
    }

    private void LoadSnapshot()
    {
        MaybeCreateSnapshotsDirectory();
        var file = Dialogs.ShowOpenFileDialog(snapshotsDirectory, "JSON files|*.json");
        if (file is null)
        {
            return;
        }
        var snapshot = JsonUtilities.LoadJson<DigiMixerSnapshot>(file);
        snapshot.CopyToViewModel(this);
    }

    private void MaybeCreateSnapshotsDirectory() => Directory.CreateDirectory(snapshotsDirectory);

    public void LogStatus(ILogger statusLogger)
    {
        if (Config.Fake)
        {
            return;
        }
        var unmutedChannels = InputChannels.Where(ic => !ic.Muted).ToList();
        statusLogger.LogTrace("Mixer {mixer}: Connected? {connected}; Unmuted channels: {channelCount}", mixer.MixerInfo.Model, mixer.Connected ? "Yes" : "No", unmutedChannels.Count);
        foreach (var channel in unmutedChannels)
        {
            statusLogger.LogTrace("  {name}: {db}", channel.DisplayName, channel.Faders.FirstOrDefault()?.FaderLevelDb.ToString("0.##"));
        }
    }
}
