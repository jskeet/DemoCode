// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Schema.Physical
{
    /// <summary>
    /// A container that only contains fields.
    /// </summary>
    public sealed class FieldContainer : ContainerBase
    {
        /// <summary>
        /// The fields within this container.
        /// </summary>
        public IReadOnlyList<IField> Fields { get; }

        /// <summary>
        /// A map from field name to fiel.
        /// </summary>
        public IReadOnlyDictionary<string, IField> FieldsByName { get; }

        /// <summary>
        /// The size of this container, in bytes.
        /// </summary>
        public int Size { get; }

        internal FieldContainer(ModuleSchema schema, string name, string description, ModuleAddress address, string path,
            int size, IReadOnlyList<IField> fields)
            : base(schema, name, description, address, path) =>
            (Size, Fields, FieldsByName) = (size, fields, fields.ToDictionary(f => f.Name).AsReadOnly());
    }
}
