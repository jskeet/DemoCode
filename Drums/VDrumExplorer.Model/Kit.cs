// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Model.Schema.Physical;

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
        public int KitNumber { get; set; }

        // TODO: This is all a mess. Fix it up.

        private Kit(ModuleData data, int kitNumber) =>
            (Schema, Data, KitNumber, KitRoot) = (data.PhysicalRoot.Schema, data, kitNumber, data.PhysicalRoot.Schema.KitRoots[1]);

        public static Kit Create(ModuleSchema moduleSchema, Dictionary<ModuleAddress, byte[]> data, int kitNumber)
        {
            var moduleData = ModuleData.FromData(moduleSchema.KitRoots[kitNumber], data);
            return new Kit(moduleData, kitNumber);
        }
    }
}
