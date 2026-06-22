namespace DigiMixer.Wpf;

public class MidiDevicePickerViewModel(IReadOnlyList<string> deviceNames)
{
    public IReadOnlyList<string> DeviceNames => deviceNames;

    // No need for event notifications here, as only the UI will set this.
    public string SelectedName { get; set; }
}
