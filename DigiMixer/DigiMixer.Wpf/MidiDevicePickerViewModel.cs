namespace DigiMixer.Wpf;

public class MidiDevicePickerViewModel
{
    public IReadOnlyList<string> DeviceNames { get; }

    // No need for event notifications here, as only the UI will set this.
    public string SelectedName { get; set; }

    public MidiDevicePickerViewModel(IReadOnlyList<string> deviceNames)
    {
        DeviceNames = deviceNames;
    }
}
