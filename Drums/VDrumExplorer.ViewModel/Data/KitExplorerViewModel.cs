// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.IO;
using VDrumExplorer.Model;
using VDrumExplorer.Proto;

namespace VDrumExplorer.ViewModel.Data
{
    public class KitExplorerViewModel : DataExplorerViewModel
    {
        public Kit Kit { get; }

        public KitExplorerViewModel(Kit kit) : base(kit.Data)
        {
            Kit = kit;
        }

        protected override string ExplorerName =>  "Module Explorer";
        public override string SaveFileFilter => "V-Drum Explorer kit files|*.vkit";

        protected override void SaveToStream(Stream stream) => Kit.Save(stream);
    }
}
