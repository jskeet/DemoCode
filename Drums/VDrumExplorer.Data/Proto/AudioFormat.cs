// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Proto
{
    internal partial class AudioFormat
    {
        internal Audio.AudioFormat ToModel() =>
            new Audio.AudioFormat(Frequency, Channels, Bits);

        internal static AudioFormat FromModel(Audio.AudioFormat format) =>
            new AudioFormat
            {
                Frequency = format.Frequency,
                Channels = format.Channels,
                Bits = format.Bits
            };
    }
}
