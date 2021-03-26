// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace XTouchMini.Model
{
    /// <summary>
    /// The style of lighting on the ring around a knob.
    /// </summary>
    public enum KnobRingStyle
    {
        /// <summary>
        /// A single light (or in standard mode, potentially two lights
        /// for mid-range values).
        /// </summary>
        Single = 0,

        /// <summary>
        /// Only valid for standard mode, and displays similarly to <see cref="Single"/>, but ensures
        /// that there is always at least one light on (whereas when the position is 0, all
        /// lights are off in Single).
        /// </summary>
        Pan = 1,

        /// <summary>
        /// Illuminates all lights from the left-most one to the specified value.
        /// </summary>
        Fan = 2,

        /// <summary>
        /// Illuminates lights from the center to whatever value is specified (so
        /// either only on the right, or only on the left).
        /// </summary>
        Spread = 3,

        /// <summary>
        /// Illuminates lights from the center, symmetrically - so 0 indicates
        /// no lights, 1 indiciates just the central light, 2 indicates the top three
        /// lights, etc.
        /// </summary>
        Trim = 4
    }
}
