// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Fields
{
    public interface IField
    {
        ModuleSchema Schema { get; }
        string Description { get; }
        FieldCondition? Condition { get; }

        /// <summary>
        /// The unique name of this field within its parent container.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///  The offset of this field relative to its parent container.
        /// </summary>
        public int Offset { get; }
    }
}
