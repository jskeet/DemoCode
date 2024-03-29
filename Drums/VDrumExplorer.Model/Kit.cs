﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using VDrumExplorer.Model.Data;
using VDrumExplorer.Model.Data.Logical;
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
        // Note: we could potentially validate here, and implement INotifyPropertyChanged. It's just as simple
        // to do so in a view model though, really.
        public int DefaultKitNumber { get; set; }

        public Kit(ModuleData data, int defaultKitNumber) =>
            (Schema, Data, DefaultKitNumber, KitRoot) =
            (data.Schema, data, defaultKitNumber, data.Schema.Kit1Root);

        /// <summary>
        /// Creates a kit from the data in a snapshot.
        /// </summary>
        /// <param name="moduleSchema">The schema of the module that this kit comes from.</param>
        /// <param name="snapshot">Snapshot of data in the kit, expected to start at the root of the normal "kit 1".</param>
        /// <param name="kitNumber">The (1-based) number of this kit.</param>
        public static Kit FromSnapshot(ModuleSchema moduleSchema, ModuleDataSnapshot snapshot, int kitNumber, ILogger logger)
        {
            var moduleData = ModuleData.FromLogicalRootNode(moduleSchema.Kit1Root);
            moduleData.LoadSnapshot(snapshot, logger);
            return new Kit(moduleData, kitNumber);
        }

        public string GetKitName() => GetKitName(Data, KitRoot);

        /// <summary>
        /// Returns the name of a kit from the specified data, given a logical kit root node.
        /// </summary>
        /// <param name="data">The data to load the name from.</param>
        /// <param name="logicalKitRoot">The logical tree node representing the root of the kit.</param>
        /// <returns>The kit name.</returns>
        internal static string GetKitName(ModuleData data, TreeNode logicalKitRoot)
        {
            var schema = data.Schema;
            var container = logicalKitRoot.Container;
            var nameField = container.ResolveField(schema.KitNamePath);
            var name = data.GetDataField(nameField).FormattedText;
            if (schema.KitSubNamePath is null)
            {
                return name;
            }
            var subNameField = container.ResolveField(schema.KitSubNamePath);
            var subName = data.GetDataField(subNameField).FormattedText;
            return subName == "" ? name : $"{name} / {subName}";
        }

        /// <summary>
        /// Returns the name of a kit from the specified data, given a logical kit root node.
        /// </summary>
        /// <param name="kitRoot">The logical data tree node representing the root of the kit.</param>
        /// <returns>The kit name.</returns>
        public static string GetKitName(DataTreeNode kitRoot) => GetKitName(kitRoot.Data, kitRoot.SchemaNode);
    }
}
