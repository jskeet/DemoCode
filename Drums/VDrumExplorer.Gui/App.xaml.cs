// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.IO;
using System.Windows;
using VDrumExplorer.Midi;
using VDrumExplorer.Model;
using VDrumExplorer.Proto;
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
        private ExplorerHomeViewModel appContext;

        public App()
        {
            appContext = new ExplorerHomeViewModel();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);            
            MainWindow = CreateModuleExplorer();
            MainWindow.Show();
            appContext.LogVersion(GetType());
            await appContext.DetectModule();
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
            var module = (Module) ProtoIo.LoadModel(Path.Combine(cwd.FullName, "td27.vdrum"));
            return new ModuleExplorer { DataContext = new DataExplorerViewModel(module.Data) };
        }

        private Window CreateSchemaExplorer()
        {
            var schema = ModuleSchema.KnownSchemas[ModuleIdentifier.TD27].Value;
            return new SchemaExplorer { DataContext = new ModuleSchemaViewModel(schema) };
        }

        private Window CreateExplorerHome()
        {
            return new ExplorerHome { DataContext = appContext };
        }
    }
}
