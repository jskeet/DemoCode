// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Proto
{
    internal partial class AudioFormat
    {
        internal Model.Audio.AudioFormat ToModel() =>
            new Model.Audio.AudioFormat(Frequency, Channels, Bits);

        internal static AudioFormat FromModel(Model.Audio.AudioFormat format) =>
            new AudioFormat
            {
                Frequency = format.Frequency,
                Channels = format.Channels,
                Bits = format.Bits
            };
    }
}
