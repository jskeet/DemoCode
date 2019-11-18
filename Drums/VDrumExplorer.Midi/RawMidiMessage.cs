// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Midi
{
    /// <summary>
    /// A raw, uninterpreted MIDI 
    /// </summary>
    internal sealed class RawMidiMessage
    {
        internal byte[] Data { get; }

        internal RawMidiMessage(byte[] data) => Data = data;

        /// <summary>
        /// The status byte of the message, which is the first byte of data.
        /// </summary>
        internal int Status => Data[0];
    }
}
