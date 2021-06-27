// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace IconPlatform.Model
{
    /// <summary>
    /// Information provided when a button is pressed or released.
    /// </summary>
    public sealed class ButtonEventArgs
    {
        /// <summary>
        /// The 1-based channel containing the button.
        /// </summary>
        public int Channel { get; }

        /// <summary>
        /// The button type.
        /// </summary>
        public ButtonType Button { get; }

        /// <summary>
        /// True if this event is for a button being pressed; false if
        /// it is for a button being released.
        /// </summary>
        public bool Down { get; }

        public ButtonEventArgs(int channel, ButtonType button, bool down) =>
            (Channel, Button, Down) = (channel, button, down);
    }
}
