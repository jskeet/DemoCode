// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace XTouchMini.Model
{
    /// <summary>
    /// Information provided when the fader is moved.
    /// </summary>
    public sealed class FaderEventArgs
    {
        /// <summary>
        /// Position of the fader, in the range 0-127.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Logical layer of the fader (or None in Mackie Control Mode).
        /// </summary>
        public Layer Layer { get; }

        public FaderEventArgs(Layer layer, int position) =>
            (Layer, Position) = (layer, position);
    }
}
