﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Windows;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.ViewModel;
using VDrumExplorer.ViewModel.Home;
using VDrumExplorer.ViewModel.LogicalSchema;

namespace VDrumExplorer.Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private SharedViewModel sharedViewModel;

        public App()
        {
            sharedViewModel = new SharedViewModel();
            DispatcherUnhandledException += (sender, args) =>
            {
                sharedViewModel.Log("Unhandled exception", args.Exception);
                args.Handled = true;
            };
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var viewModel = new ExplorerHomeViewModel(ViewServices.Instance, sharedViewModel);
            MainWindow = new ExplorerHome { DataContext = viewModel };
            MainWindow.Show();
            sharedViewModel.LogVersion(GetType());
            await sharedViewModel.DetectModule();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            sharedViewModel?.ConnectedDevice?.Dispose();
        }

        // TODO: Need to get at this somewhere...
        private Window CreateSchemaExplorer()
        {
            var schema = ModuleSchema.KnownSchemas[ModuleIdentifier.TD27].Value;
            return new SchemaExplorer { DataContext = new ModuleSchemaViewModel(schema) };
        }
    }
}
