// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using VDrumExplorer.Model.Fields;

namespace VDrumExplorer.Model.PhysicalSchema
{
    /// <summary>
    /// A container that only contains fields.
    /// </summary>
    public class FieldContainer : ContainerBase
    {
        /// <summary>
        /// The fields within this container.
        /// </summary>
        public IReadOnlyList<IField> Fields { get; }

        /// <summary>
        /// The size of this container, in bytes.
        /// </summary>
        public int Size { get; }

        internal FieldContainer(string name, string description, ModuleAddress address, string path,
            int size, IReadOnlyList<IField> fields)
            : base(name, description, address, path) =>
            (Size, Fields) = (size, fields);
    }
}
