// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Linq;
using System.Windows.Forms;
using VDrumExplorer.Data;

namespace VDrumExplorer.Gui
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Validate nice and early...
            SchemaRegistry.KnownSchemas.Values.Select(lazy => lazy.Value).ToList();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ModuleLoader());
        }
    }
}
