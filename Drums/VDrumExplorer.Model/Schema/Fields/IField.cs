// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Model.Schema.Fields
{
    /// <summary>
    /// Interface implemented by all fields.
    /// </summary>
    public interface IField
    {
        /// <summary>
        /// The offset of this field within its parent container.
        /// </summary>
        ModuleOffset Offset { get; }

        /// <summary>
        /// Size of this field in bytes.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Name of the field. (This forms the last part of the full path of the field.)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Description of the field.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The condition associated with the field, if any.
        /// </summary>
        public Condition? Condition { get; }
    }
}
