// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Threading;
using System.Threading.Tasks;

namespace VDrumExplorer.Model.Audio
{
    public interface IAudioOutput
    {
        string Name { get; }
        Task PlayAudioAsync(AudioFormat format, byte[] bytes, CancellationToken cancellationToken);
    }
}
