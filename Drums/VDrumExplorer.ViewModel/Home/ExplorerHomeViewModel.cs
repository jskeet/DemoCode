// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.ViewModel.Home
{
    public class ExplorerHomeViewModel : ViewModelBase
    {
        public SharedViewModel SharedViewModel { get; }

        public LogViewModel Log => SharedViewModel.LogViewModel;

        public ExplorerHomeViewModel(SharedViewModel shared) =>
            SharedViewModel = shared;

        private int loadKitFromDeviceNumber = 1;
        public int LoadKitFromDeviceNumber
        {
            get => loadKitFromDeviceNumber;
            set => SetProperty(ref loadKitFromDeviceNumber, SharedViewModel.ConnectedDeviceSchema.ValidateKitNumber(value));
        }
    }
}
