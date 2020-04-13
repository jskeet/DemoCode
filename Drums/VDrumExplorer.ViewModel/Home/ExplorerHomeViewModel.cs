// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;

namespace VDrumExplorer.ViewModel.Home
{
    public class ExplorerHomeViewModel : ViewModelBase
    {
        public LogViewModel Log { get; }

        public ModuleSchema? ConnectedDeviceSchema { get; private set; }

        internal RolandMidiClient? ConnectedDevice { get; private set; }

        public ExplorerHomeViewModel()
        {
            Log = new LogViewModel();
        }

        public void LogVersion(Type type)
        {
            var version = type.Assembly.GetCustomAttributes().OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
            if (version != null)
            {
                Log.Log($"V-Drum Explorer version {version.InformationalVersion}");
            }
            else
            {
                Log.Log($"Version attribute not found.");
            }
        }

        public async Task DetectModule()
        {
            ConnectedDevice = await MidiDevices.DetectRolandMidiClientAsync(Log.Log, ModuleSchema.KnownSchemas.Keys);
            ConnectedDeviceSchema = ConnectedDevice is null ? null : ModuleSchema.KnownSchemas[ConnectedDevice.Identifier].Value;
            RaisePropertyChanged(nameof(ConnectedDevice));
            RaisePropertyChanged(nameof(ConnectedDeviceSchema));
        }
    }
}
