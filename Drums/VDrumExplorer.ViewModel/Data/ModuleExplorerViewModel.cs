// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.IO;
using VDrumExplorer.Model;
using VDrumExplorer.Proto;

namespace VDrumExplorer.ViewModel.Data
{
    public class ModuleExplorerViewModel : DataExplorerViewModel
    {
        public Module Module { get; }

        public ModuleExplorerViewModel(SharedViewModel shared, Module module) : base(shared, module.Data)
        {
            Module = module;
        }

        protected override string ExplorerName =>  "Module Explorer";
        public override string SaveFileFilter => "V-Drum Explorer module files|*.vdrum";

        protected override void SaveToStream(Stream stream) => Module.Save(stream);
    }
}
