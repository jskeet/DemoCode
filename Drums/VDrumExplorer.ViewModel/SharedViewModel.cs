// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.ViewModel.Home;

namespace VDrumExplorer.ViewModel
{
    /// <summary>
    /// Information shared by multiple view models, e.g. the log and connected device.
    /// </summary>
    public class SharedViewModel : ViewModelBase
    {
        public LogViewModel LogViewModel { get; }

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

        public SharedViewModel()
        {
            LogViewModel = new LogViewModel();
        }

        public void Log(string text)
        {
            LogViewModel.Log(text);
        }

        public void Log(string text, Exception exception)
        {
            LogViewModel.Log($"{text}: {exception.GetType().Name}: {exception.Message}");
        }

        public void LogVersion(Type type)
        {
            var version = type.Assembly.GetCustomAttributes().OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
            if (version != null)
            {
                Log($"V-Drum Explorer version {version.InformationalVersion}");
            }
            else
            {
                Log($"Version attribute not found.");
            }
        }

        public async Task DetectModule()
        {
            ConnectedDevice = await MidiDevices.DetectSingleRolandMidiClientAsync(Log, ModuleSchema.KnownSchemas.Keys);
            ConnectedDeviceSchema = ConnectedDevice is null ? null : ModuleSchema.KnownSchemas[ConnectedDevice.Identifier].Value;
            RaisePropertyChanged(nameof(ConnectedDevice));
            RaisePropertyChanged(nameof(ConnectedDeviceSchema));
        }

    }
}
