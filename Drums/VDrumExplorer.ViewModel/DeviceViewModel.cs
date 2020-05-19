// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;

namespace VDrumExplorer.ViewModel
{
    public sealed class DeviceViewModel : ViewModelBase
    {
        private ModuleSchema? connectedDeviceSchema;
        public ModuleSchema? ConnectedDeviceSchema
        {
            get => connectedDeviceSchema;
            set => SetProperty(ref connectedDeviceSchema, value);
        }

        private RolandMidiClient? connectedDevice;
        public RolandMidiClient? ConnectedDevice
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
            ConnectedDevice = await MidiDevices.DetectSingleRolandMidiClientAsync(logger, ModuleSchema.KnownSchemas.Keys);
            ConnectedDeviceSchema = ConnectedDevice is null ? null : ModuleSchema.KnownSchemas[ConnectedDevice.Identifier].Value;
            RaisePropertyChanged(nameof(ConnectedDevice));
            RaisePropertyChanged(nameof(ConnectedDeviceSchema));
        }
    }
}
