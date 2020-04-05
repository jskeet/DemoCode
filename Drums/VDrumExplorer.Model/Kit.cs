// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.LogicalSchema;

namespace VDrumExplorer.Model
{
    /// <summary>
    /// A kit is part of a module. It has a complete <see cref="ModuleSchema"/>, but all the data is within one part of it.
    /// </summary>
    public class Kit
    {
        public ModuleSchema Schema { get; }
        public ModuleData Data { get; }
        public TreeNode KitRoot { get; }
        public int KitNumber { get; set; }

        public Kit(ModuleSchema schema, ModuleData data, int kitNumber) =>
            (Schema, Data, KitNumber, KitRoot) = (schema, data, kitNumber, schema.KitRoots[1]);
    }
}
