// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using VDrumExplorer.Model.Data;

namespace VDrumExplorer.Model
{
    /// <summary>
    /// A module consists of a <see cref="ModuleData"/> and
    /// the <see cref="ModuleSchema"/> that the data abides by.
    /// </summary>
    public sealed class Module
    {
        public ModuleSchema Schema { get; }
        public ModuleData Data { get; }

        public Module(ModuleData data) => (Schema, Data) = (data.Schema, data);

        public static Module FromSnapshot(ModuleSchema moduleSchema, ModuleDataSnapshot snapshot)
        {
            var moduleData = ModuleData.FromLogicalRootNode(moduleSchema.LogicalRoot);
            moduleData.LoadSnapshot(snapshot);
            return new Module(moduleData);
        }

        public string GetKitName(int kitNumber) => Kit.GetKitName(Data, Schema.KitRoots[kitNumber - 1]);

        /// <summary>
        /// Copies the data in the specified kit into this module, at the specified kit number.
        /// </summary>
        /// <param name="kit">The kit data to import. Must have the same schema as this module.</param>
        /// <param name="kitNumber">The number of the kit to replace within this momdule.</param>
        public void ImportKit(Kit kit, int kitNumber)
        {
            if (kit.Schema != Schema)
            {
                throw new ArgumentException($"Module and kit schemas do not match");
            }
            var targetRoot = Schema.KitRoots[kitNumber - 1];
            var kitData = kit.Data.CreateSnapshot().Relocated(kit.KitRoot, targetRoot);
            Data.LoadPartialSnapshot(kitData);
        }

        /// <summary>
        /// Creates a copy of the specified kit. The default kit number
        /// is initialized to be the same as the original kit number.
        /// </summary>
        /// <param name="kitNumber">The kit number</param>
        /// <returns>The extracted kit.</returns>
        public Kit ExportKit(int kitNumber)
        {
            var root = Schema.KitRoots[kitNumber - 1];
            var target = Schema.KitRoots[0];
            var snapshot = Data.CreatePartialSnapshot(root);
            snapshot = snapshot.Relocated(root, target);
            return Kit.FromSnapshot(Schema, snapshot, kitNumber);
        }
    }
}
