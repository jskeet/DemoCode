using System.Collections.Immutable;

namespace DigiMixer.Wpf;

public class MidiDevicePickerViewModel(ImmutableArray<string> deviceNames)
{
    public ImmutableArray<string> DeviceNames => deviceNames;

    // No need for event notifications here, as only the UI will set this.
    public string? SelectedName { get; set; }
}
