// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace XTouchMini.Model
{
    /// <summary>
    /// Information provided when a knob is turned.
    /// </summary>
    public sealed class KnobTurnedEventArgs
    {
        /// <summary>
        /// The knob number, in the range 1-8.
        /// </summary>
        public int Knob { get; }

        /// <summary>
        /// The logical layer of the knob being turned (or None in Mackie Control Mode).
        /// </summary>
        public Layer Layer { get; }

        /// <summary>
        /// The reported knob value, in the range 0-127.
        /// In standard mode this is the knob position in the range 0-127.
        /// In Mackie Control Mode this is the velocity: values 0x01-0x07
        /// are clockwise, and values 0x41-0x47 are counter-clockwise.
        /// </summary>
        public int Value { get; }

        public KnobTurnedEventArgs(int knob, Layer layer, int value) =>
            (Knob, Layer, Value) = (knob, layer, value);
    }
}
