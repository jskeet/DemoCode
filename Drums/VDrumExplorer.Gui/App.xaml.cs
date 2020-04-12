// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.IO;
using System.Windows;
using VDrumExplorer.Model;
using VDrumExplorer.Proto;
using VDrumExplorer.ViewModel.Data;
using VDrumExplorer.ViewModel.LogicalSchema;

namespace VDrumExplorer.Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShowModuleExplorer();
        }

        private void ShowModuleExplorer()
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
            MainWindow = new ModuleExplorer { DataContext = new DataExplorerViewModel(module.Data) };
            MainWindow.Show();
        }

        private void ShowSchemaExplorer()
        {
            var schema = ModuleSchema.KnownSchemas[ModuleIdentifier.TD27].Value;
            MainWindow = new SchemaExplorer { DataContext = new ModuleSchemaViewModel(schema) };
            MainWindow.Show();
        }
    }
}
