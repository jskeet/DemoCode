// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDrumExplorer.Data.Fields;
using VDrumExplorer.Data.Layout;
using VDrumExplorer.Data.Proto;

namespace VDrumExplorer.Data
{
    /// <summary>
    /// A kit is part of a module. It has a complete <see cref="ModuleSchema"/>, but all the data is within one part of it.
    /// </summary>
    public class Kit
    {
        public ModuleSchema Schema { get; }
        public ModuleData Data { get; }
        public VisualTreeNode KitRoot { get; }
        public int DefaultKitNumber { get; }

        public Kit(ModuleSchema schema, ModuleData data, int defaultKitNumber) =>
            (Schema, Data, DefaultKitNumber, KitRoot) = (schema, data, defaultKitNumber, schema.KitRoots[1]);

        /// <summary>
        /// Validates that every field in the schema has a valid value.
        /// This will fail if only partial data has been loaded.
        /// </summary>
        public ValidationResult Validate() => KitRoot.Context.ValidateDescendantsAndSelf(Data);

        public void Save(Stream stream) => ProtoIo.Write(stream, this);
    }
}
