// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.ViewModel.Data
{
    // Experiment: use a simple interface here instead of a base class.
    public interface IDataNodeDetailViewModel
    {
        public string Description { get; }
    }
}
