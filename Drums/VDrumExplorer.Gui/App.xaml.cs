// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using System.Windows;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Audio;
using VDrumExplorer.NAudio;
using VDrumExplorer.ViewModel;
using VDrumExplorer.ViewModel.Logging;
using VDrumExplorer.ViewModel.LogicalSchema;

namespace VDrumExplorer.Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IAudioDeviceManager audioDeviceManager;
        private DeviceViewModel deviceViewModel;
        private LogViewModel logViewModel;

        public App()
        {
            audioDeviceManager = new NAudioDeviceManager();
            deviceViewModel = new DeviceViewModel();
            logViewModel = new LogViewModel();
            DispatcherUnhandledException += (sender, args) =>
            {
                logViewModel.Logger.LogError(args.Exception, "Unhandled exception");
                args.Handled = true;
            };
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var viewModel = new ExplorerHomeViewModel(ViewServices.Instance, logViewModel, deviceViewModel, audioDeviceManager);
            MainWindow = new ExplorerHome { DataContext = viewModel };
            MainWindow.Show();
            logViewModel.LogVersion(GetType());
            await deviceViewModel.DetectModule(logViewModel.Logger);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            deviceViewModel?.ConnectedDevice?.Dispose();
        }

        // TODO: Need to get at this somewhere...
        private Window CreateSchemaExplorer()
        {
            var schema = ModuleSchema.KnownSchemas[ModuleIdentifier.TD27].Value;
            return new SchemaExplorer { DataContext = new ModuleSchemaViewModel(schema) };
        }
    }
}
