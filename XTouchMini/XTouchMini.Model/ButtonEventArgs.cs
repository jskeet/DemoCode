// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace XTouchMini.Model
{
    /// <summary>
    /// Information provided when a button is pressed or released.
    /// </summary>
    public sealed class ButtonEventArgs
    {
        /// <summary>
        /// The button number, in the range 1-16, or 0
        /// if a layer button has been pressed in Mackie Control Mode.
        /// </summary>
        public int Button { get; }

        /// <summary>
        /// The layer of the button (or None in Mackie Control Mode, except
        /// for when the actual layer buttons have been pressed).
        /// </summary>
        public Layer Layer { get; }

        /// <summary>
        /// True if this event is for a button being pressed; false if
        /// it is for a button being released.
        /// </summary>
        public bool Down { get; }

        public ButtonEventArgs(int key, Layer layer, bool down) =>
            (Button, Layer, Down) = (key, layer, down);
    }
}
