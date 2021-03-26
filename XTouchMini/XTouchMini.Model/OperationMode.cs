// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace XTouchMini.Model
{
    /// <summary>
    /// The operation mode of the device.
    /// </summary>
    public enum OperationMode : byte
    {
        /// <summary>
        /// Standard MIDI mode. In this mode, while the lights around the knobs
        /// and in the buttons can be controlled via software, they are also modified
        /// automatically, e.g. by pressing the buttons. Therefore this mode has
        /// less direct control - but is simpler to integrate with when custom display
        /// is not required.
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Mackie Control Mode, providing more control over the visuals, but with
        /// less built-in behavior. The knobs do not have a logical "position", instead
        /// only reporting the velocity with which they are turned.
        /// </summary>
        MackieControl = 1
    }
}
