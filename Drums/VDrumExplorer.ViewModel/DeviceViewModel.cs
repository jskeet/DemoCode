// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Device;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.ViewModel
{
    public sealed class DeviceViewModel : ViewModelBase
    {
        public string ConnectedDeviceName => ConnectedDevice?.Schema.Identifier.Name ?? "(None)";

        private DeviceController? connectedDevice;
        public DeviceController? ConnectedDevice
        {
            get => connectedDevice;
            set
            {
                if (SetProperty(ref connectedDevice, value))
                {
                    RaisePropertyChanged(nameof(DeviceConnected));
                    RaisePropertyChanged(nameof(ConnectedDeviceName));
                }
            }
        }

        public bool DeviceConnected => connectedDevice is object;

        public async Task DetectModule(ILogger logger)
        {
            var client = await MidiDevices.DetectSingleRolandMidiClientAsync(logger, ModuleSchema.KnownSchemas.Keys);
            ConnectedDevice = client is null ? null : new DeviceController(client, logger);
            RaisePropertyChanged(nameof(ConnectedDevice));
        }
    }
}
