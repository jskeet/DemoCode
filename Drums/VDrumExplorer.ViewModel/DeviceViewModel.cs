// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Device;

namespace VDrumExplorer.ViewModel
{
    public sealed class DeviceViewModel : ViewModelBase
    {
        private DeviceController? connectedDevice;
        public DeviceController? ConnectedDevice
        {
            get => connectedDevice;
            set
            {
                if (SetProperty(ref connectedDevice, value))
                {
                    RaisePropertyChanged(nameof(DeviceConnected));
                }
            }
        }

        public bool DeviceConnected => connectedDevice is object;

        public async Task DetectModule(ILogger logger)
        {
            var client = await MidiDevices.DetectSingleRolandMidiClientAsync(logger, ModuleSchema.KnownSchemas.Keys);
            ConnectedDevice = client is null ? null : new DeviceController(client);
            RaisePropertyChanged(nameof(ConnectedDevice));
        }
    }
}
