// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.IO;
using System.Windows;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Proto;
using VDrumExplorer.ViewModel;
using VDrumExplorer.ViewModel.Data;
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
            
            MainWindow = CreateExplorerHome();
            MainWindow.Show();
            CreateModuleExplorer().Show();
            sharedViewModel.LogVersion(GetType());
            await sharedViewModel.DetectModule();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            sharedViewModel?.ConnectedDevice?.Dispose();
        }

        private Window CreateModuleExplorer()
        {
            // FIXME: This is just for development...
            var cwd = new DirectoryInfo(".");
            while (cwd.Name != "Drums")
            {
                if (cwd.Parent is null)
                {
                    throw new InvalidOperationException("Can't find Drums directory");
                }
                cwd = cwd.Parent;
            }
            var path = Path.Combine(cwd.FullName, "td27.vdrum");
            var module = (Module) ProtoIo.LoadModel(path);
            return new DataExplorer { DataContext = new ModuleExplorerViewModel(sharedViewModel, module) { FileName = path } };
        }

        private Window CreateSchemaExplorer()
        {
            var schema = ModuleSchema.KnownSchemas[ModuleIdentifier.TD27].Value;
            return new SchemaExplorer { DataContext = new ModuleSchemaViewModel(schema) };
        }

        private Window CreateExplorerHome()
        {
            return new ExplorerHome { DataContext = new ExplorerHomeViewModel(sharedViewModel) };
        }
    }
}
