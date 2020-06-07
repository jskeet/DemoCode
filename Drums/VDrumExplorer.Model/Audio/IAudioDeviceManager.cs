// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Model.Audio
{
    public interface IAudioDeviceManager
    {
        IReadOnlyList<IAudioInput> GetInputs();
        IReadOnlyList<IAudioOutput> GetOutputs();
    }
}
