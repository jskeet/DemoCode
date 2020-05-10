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

        public int DefaultKitNumber
        {
            get => Kit.DefaultKitNumber;
            set
            {
                Kit.DefaultKitNumber = Kit.Schema.ValidateKitNumber(value);
                RaisePropertyChanged(nameof(DefaultKitNumber));
            }
        }

        private int kitCopyTargetNumber;
        public int KitCopyTargetNumber
        {
            get => kitCopyTargetNumber;
            set => SetProperty(ref kitCopyTargetNumber, Kit.Schema.ValidateKitNumber(value));
        }

        public KitExplorerViewModel(SharedViewModel shared, Kit kit) : base(shared, kit.Data)
        {
            Kit = kit;
            kitCopyTargetNumber = kit.DefaultKitNumber;
        }

        protected override string ExplorerName =>  "Module Explorer";
        public override string SaveFileFilter => "V-Drum Explorer kit files|*.vkit";

        protected override void SaveToStream(Stream stream) => Kit.Save(stream);
    }
}
