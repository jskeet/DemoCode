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
                if (value < 1 || value >= module.Schema.KitRoots.Count)
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

        public string DestinationKitName => module.GetKitName(destinationKitNumber);

        public bool CopyEnabled => kit.DefaultKitNumber != destinationKitNumber;

        public CopyKitViewModel(Module module, Kit kit)
        {
            this.module = module;
            this.kit = kit;
            SourceKitName = kit.GetKitName();
        }
    }
}
