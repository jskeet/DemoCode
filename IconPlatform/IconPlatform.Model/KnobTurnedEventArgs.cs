// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace IconPlatform.Model
{
    /// <summary>
    /// Information provided when a knob is turned.
    /// </summary>
    public sealed class KnobTurnedEventArgs
    {
        public int Channel { get; }

        /// <summary>
        /// The reported knob value, in the range 0-127.
        /// In standard mode this is the knob position in the range 0-127.
        /// In Mackie Control Mode this is the velocity: values 0x01-0x07
        /// are clockwise, and values 0x41-0x47 are counter-clockwise.
        /// </summary>
        public int Value { get; }

        public KnobTurnedEventArgs(int channel, int value) => (Channel, Value) = (channel, value);
    }
}
