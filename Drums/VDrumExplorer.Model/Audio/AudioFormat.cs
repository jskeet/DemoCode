// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Model.Audio
{
    /// <summary>
    /// The format of an audio sample.
    /// </summary>
    public sealed class AudioFormat
    {
        /// <summary>
        /// The frequency in Hz.
        /// </summary>
        public int Frequency { get; }

        /// <summary>
        /// The number of channels (typically 1 or 2).
        /// </summary>
        public int Channels { get; }

        /// <summary>
        /// The number of bits per sample.
        /// </summary>
        public int Bits { get; }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="frequency">The frequency in Hz.</param>
        /// <param name="channels">The number of channels.</param>
        /// <param name="bits">The number of bits per sample.</param>
        public AudioFormat(int frequency, int channels, int bits) =>
            (Frequency, Channels, Bits) = (frequency, channels, bits);

        /// <summary>
        /// The number of bytes recorded per second.
        /// </summary>
        public int BytesPerSecond => Frequency * Channels * Bits / 8;
    }
}
