using Commons.Music.Midi;
using DigiMixer.Controls;
using JonSkeet.WpfUtil;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Windows.Input;
using MHT = DigiMixer.Controls.DigiMixerConfig.MixerHardwareType;

namespace DigiMixer.Wpf;

public class ConfigurationEditorViewModel : ViewModelBase<DigiMixerAppConfig>
{
    private const string InputPrefix = "input-";
    private const string OutputPrefix = "output-";
    private readonly ILogger logger;

    private static List<(string, MHT)> allMixerTypes = new()
    {
        ("Behringer X-Air (XR-12, XR-16, XR-18)", MHT.XAir),
        ("Behringer X-32 / Midas M-32", MHT.X32),
        ("Mackie DL series", MHT.MackieDL),
        ("Allen & Heath Qu series", MHT.AllenHeathQu),
        ("Allen & Heath Cq series", MHT.AllenHeathCq),
        ("Yamaha DM series", MHT.YamahaDm),
        ("PreSonus StudioLive Series III", MHT.StudioLive),
        ("Soundcraft Ui series", MHT.SoundcraftUi),
        ("Fake", MHT.Fake)
    };

    public string Address
    {
        get => Model.Mixer.Address;
        set => SetProperty(Address, value, x => Model.Mixer.Address = x);
    }

    public int? Port
    {
        get => Model.Mixer.Port;
        set => SetProperty(Port, value, x => Model.Mixer.Port = x);
    }

    public bool EnablePeripherals
    {
        get => Model.EnablePeripherals;
        set => SetProperty(EnablePeripherals, value, x => Model.EnablePeripherals = x);
    }

    public string XTouchMiniDevice
    {
        get => Model.Mixer.XTouchMiniDevice;
        set => SetProperty(XTouchMiniDevice, value, x => Model.Mixer.XTouchMiniDevice = x);
    }

    public string IconMPlusDevice
    {
        get => Model.Mixer.IconMPlusDevice;
        set => SetProperty(IconMPlusDevice, value, x => Model.Mixer.IconMPlusDevice = x);
    }

    public string IconXPlusDevice
    {
        get => Model.Mixer.IconXPlusDevice;
        set => SetProperty(IconXPlusDevice, value, x => Model.Mixer.IconXPlusDevice = x);
    }

    public ChannelListViewModel InputChannels { get; } = new(InputPrefix);
    public ChannelListViewModel OutputChannels { get; } = new(OutputPrefix);

    public List<string> AllMixerTypes => allMixerTypes.Select(pair => pair.Item1).ToList();

    public string SelectedMixerType
    {
        get => allMixerTypes.FirstOrDefault(pair => pair.Item2 == Model.Mixer.HardwareType).Item1 ?? "Unknown";
        set => SetProperty(SelectedMixerType, value,
            // This will end up using "fake" by default, which is unfortunate but not awful.
            x => Model.Mixer.HardwareType = allMixerTypes.FirstOrDefault(pair => pair.Item1 == x).Item2);
    }

    public ICommand TestConfigurationCommand { get; }
    public ICommand PickMidiDeviceCommand { get; }

    public ConfigurationEditorViewModel(ILogger logger, DigiMixerAppConfig model) : base(model)
    {
        this.logger = logger;
        InputChannels.Mappings.ReplaceContentWith(model.Mixer.InputChannels.Select(x => new ChannelMappingViewModel(x)));
        OutputChannels.Mappings.ReplaceContentWith(model.Mixer.OutputChannels.Select(x => new ChannelMappingViewModel(x)));
        TestConfigurationCommand = ActionCommand.FromAction(TestConfiguration);
        PickMidiDeviceCommand = ActionCommand.FromAction<string>(PickMidiDevice);
    }

    internal void UpdateModel()
    {
        ReplaceMappings(Model.Mixer.InputChannels, InputChannels);
        ReplaceMappings(Model.Mixer.OutputChannels, OutputChannels);

        void ReplaceMappings(List<ChannelMapping> list, ChannelListViewModel vm)
        {
            list.Clear();
            list.AddRange(vm.Mappings.Select(mapping => mapping.Model));
        }
    }

    private async Task TestConfiguration()
    {
        Mixer mixer;
        try
        {
            // TODO: Maybe retry?
            mixer = await Mixer.Create(NullLogger.Instance, () => Model.Mixer.CreateMixerApi(NullLogger.Instance));
        }
        catch
        {
            Dialogs.ShowErrorDialog("Unable to detect mixer", "Unable to detect mixer. Please check the settings.");
            return;
        }
        // We may not actually have channel names yet... let's wait for a couple of seconds
        // to allow the "request all data" to actually complete refreshing.
        await Task.Delay(2000);

        // If we've somehow disconnected while waiting, report an error instead of continuing.
        if (!mixer.Connected)
        {
            Dialogs.ShowErrorDialog("Unable to detect mixer", "Unable to detect mixer. Please check the settings.");
            return;
        }

        var inputs = mixer.InputChannels.Where(ch => !string.IsNullOrEmpty(ch.Name)).ToList();
        var outputs = mixer.OutputChannels.Where(ch => !string.IsNullOrEmpty(ch.Name)).ToList();

        var result = Dialogs.ShowYesNoDialog("Mixer detected successfully.",
            "Mixer detected successfully, with the following channel names:\r\n" +
            $"Inputs: {string.Join(", ", inputs.Select(ch => ch.Name))}\r\n" +
            $"Outputs: {string.Join(", ", outputs.Select(ch => ch.Name))}\r\n\r\n" +
            "Would you like to map these channel names in DigiMixer?");

        if (result)
        {
            var inputMappings = inputs.Select((input, index) => new ChannelMappingViewModel(new ChannelMapping
            {
                Id = $"{InputPrefix}{index + 1}",
                DisplayName = input.Name,
                Channel = input.LeftOrMonoChannelId.Value,
                InitiallyVisible = true
            }));
            InputChannels.Mappings.ReplaceContentWith(inputMappings);

            var outputMappings = outputs.Select((output, index) => new ChannelMappingViewModel(new ChannelMapping
            {
                Id = $"{OutputPrefix}{index + 1}",
                DisplayName = output.Name,
                Channel = output.LeftOrMonoChannelId.Value,
                InitiallyVisible = true
            }));
            OutputChannels.Mappings.ReplaceContentWith(outputMappings);
        }
    }

    private void PickMidiDevice(string target)
    {
        List<string> inputs;
        List<string> outputs;
        try
        {
            var manager = MidiAccessManager.Default;
            inputs = manager.Inputs.Select(p => p.Name).ToList();
            outputs = manager.Outputs.Select(p => p.Name).ToList();
        }
        catch
        {
            Dialogs.ShowErrorDialog("Unable to load MIDI devices", "Unable to load the names of all MIDI devices. Please check your devices.");
            return;
        }
        var common = inputs.Intersect(outputs).ToList();
        if (common.Count == 0)
        {
            Dialogs.ShowWarningDialog("No suitable MIDI devices found",
                "There were no MIDI devices with the same input and output names. Please check your devices.");
            return;
        }
        var vm = new MidiDevicePickerViewModel(common);
        var dialog = new MidiDevicePickerDialog { DataContext = vm };
        if (dialog.ShowDialog() == true && vm.SelectedName is string name)
        {
            switch (target)
            {
                case "XTouch":
                    XTouchMiniDevice = name;
                    break;
                case "IconM":
                    IconMPlusDevice = name;
                    break;
                case "IconX":
                    IconXPlusDevice = name;
                    break;
                default:
                    logger.LogError("Unknown target for MIDI device name: {target}", target);
                    break;
            }
        }
    }
}
