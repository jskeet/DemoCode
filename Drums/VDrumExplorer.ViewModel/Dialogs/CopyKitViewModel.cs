// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model;
using VDrumExplorer.Model.Data.Logical;

namespace VDrumExplorer.ViewModel.Dialogs
{
    public class CopyKitViewModel : ViewModelBase
    {
        private Module module;
        private Kit kit;
        public string SourceKitName { get; }

        private int destinationKitNumber = 1;
        public int DestinationKitNumber
        {
            get => destinationKitNumber;
            set
            {
                if (destinationKitNumber < 1 || destinationKitNumber >= module.Schema.KitRoots.Count)
                {
                    throw new ArgumentOutOfRangeException("Invalid destination kit number");
                }
                if (SetProperty(ref destinationKitNumber, value))
                {
                    RaisePropertyChanged(nameof(DestinationKitName));
                    RaisePropertyChanged(nameof(CopyEnabled));
                }
            }
        }

        public string DestinationKitName
        {
            get
            {
                var destinationRoot = module.Schema.KitRoots[destinationKitNumber - 1];
                return new DataFieldFormattableString(module.Data, destinationRoot.Format).Text;
            }
        }

        public bool CopyEnabled => kit.DefaultKitNumber != destinationKitNumber;

        public CopyKitViewModel(Module module, Kit kit)
        {
            this.module = module;
            this.kit = kit;
            SourceKitName = new DataFieldFormattableString(kit.Data, kit.KitRoot.Format).Text;
        }
    }
}
