// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Model.Midi
{
    /// <summary>
    /// A raw, uninterpreted MIDI message.
    /// </summary>
    public sealed class MidiMessage
    {
        public byte[] Data { get; }
        public long Timestamp { get; }

        public MidiMessage(byte[] data) : this(data, 0L)
        {
        }

        public MidiMessage(byte[] data, long timestamp) =>
            (Data, Timestamp) = (data, timestamp);

        /// <summary>
        /// The status byte of the message, which is the first byte of data.
        /// </summary>
        public int Status => Data[0];
    }
}
