// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Schema.Logical;

namespace VDrumExplorer.Model
{
    /// <summary>
    /// A kit is part of a module. It has a complete <see cref="ModuleSchema"/>, but all the data is within one part of it.
    /// </summary>
    public sealed class Kit
    {
        public ModuleSchema Schema { get; }
        public ModuleData Data { get; }
        public TreeNode KitRoot { get; }
        public int DefaultKitNumber { get; set; }

        private Kit(ModuleData data, int defaultKitNumber) =>
            (Schema, Data, DefaultKitNumber, KitRoot) =
            (data.PhysicalRoot.Schema, data, defaultKitNumber, data.PhysicalRoot.Schema.KitRoots[0]);

        public static Kit FromSnapshot(ModuleSchema moduleSchema, ModuleDataSnapshot snapshot, int kitNumber)
        {
            var moduleData = ModuleData.FromLogicalRootNode(moduleSchema.KitRoots[0]);
            moduleData.LoadSnapshot(snapshot);
            return new Kit(moduleData, kitNumber);
        }
    }
}
