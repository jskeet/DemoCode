// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace XTouchMini.Model
{
    /// <summary>
    /// Logical layer for a button or knob in Standard mode.
    /// Also used to identify layer buttons in Mackie Control mode.
    /// </summary>
    public enum Layer
    {
        /// <summary>
        /// No layer: device is in Mackie Control Mode.
        /// </summary>
        None = 0,

        /// <summary>
        /// Layer A in standard mode.
        /// </summary>
        LayerA = 1,

        /// <summary>
        /// Layer B in standard mode.
        /// </summary>
        LayerB = 2
    }
}
