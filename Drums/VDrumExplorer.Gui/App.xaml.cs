// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Windows;
using VDrumExplorer.Model;
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
            var schema = ModuleSchema.KnownSchemas[ModuleIdentifier.TD27].Value;
            MainWindow = new SchemaExplorer { DataContext = new ModuleSchemaViewModel(schema) };
            MainWindow.Show();
        }
    }
}
