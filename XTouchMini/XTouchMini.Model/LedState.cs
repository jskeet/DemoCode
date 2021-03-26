// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace XTouchMini.Model
{
    /// <summary>
    /// Tristate type to indicate that an LED should be set to
    /// be off, on (steady) or blinking.
    /// </summary>
    public enum LedState
    {
        /// <summary>
        /// The LED should be off.
        /// </summary>
        Off,

        /// <summary>
        /// The LED should be on (steady).
        /// </summary>
        On,

        /// <summary>
        /// The LED should blink.
        /// </summary>
        Blinking
    }
}
