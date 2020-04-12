// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Model.Schema.Json
{
    /// <summary>
    /// Information about how items are repeated.
    /// </summary>
    internal sealed class RepeatJson
    {
        /// <summary>
        /// The number of times this field is repeated. May be numeric, or
        /// a variable or lookup name such as "kits" or "instruments".
        /// </summary>
        public string? Items { get; set; }

        /// <summary>
        /// The variable to introduce to represent the repetition index from
        /// within the context of the repeated field itself.
        /// </summary>
        public string? IndexVariable { get; set; }

        /// <summary>
        /// The gap between repeated fields (from the start of field X to the start of field X+1).
        /// Not required when a field (rather than a container) is being repeated, if it would be
        /// equal to the size of the field.
        /// </summary>
        public HexInt32? Gap { get; set; }
    }
}
