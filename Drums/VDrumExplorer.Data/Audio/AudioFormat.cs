// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Audio
{
    public sealed class AudioFormat
    {
        public int Frequency { get; }
        public int Channels { get; }
        public int Bits { get; }

        public AudioFormat(int frequency, int channels, int bits) =>
            (Frequency, Channels, Bits) = (frequency, channels, bits);

        public int BytesPerSecond => Frequency * Channels * Bits / 8;
    }
}
