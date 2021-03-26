// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace XTouchMini.Model
{
    /// <summary>
    /// Information provided when a knob is pressed or released.
    /// </summary>
    public sealed class KnobPressEventArgs
    {
        /// <summary>
        /// The knob number, in the range 1-8.
        /// </summary>
        public int Knob { get; }

        /// <summary>
        /// The layer of the knob (or None in Mackie Control Mode).
        /// </summary>
        public Layer Layer { get; }

        /// <summary>
        /// True if this event is for a knob being pressed; false if
        /// it is for a knob being released.
        /// </summary>
        public bool Down { get; }

        public KnobPressEventArgs(int knob, Layer layer, bool down) =>
            (Knob, Layer, Down) = (knob, layer, down);
    }
}
