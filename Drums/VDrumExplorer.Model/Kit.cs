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
        // Note: we could potentially validate here, and implement INotifyPropertyChanged. It's just as simple
        // to do so in a view model though, really.
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

        public string GetKitName() => GetKitName(Data, KitRoot);

        // TODO: Fix this, because hardcoding it is horrible. However, putting something general
        // into all schema nodes is pretty unpleasant too. Maybe a top-level piece of JSON?

        /// <summary>
        /// Returned the name of a kit from the specified data, given a logical kit root node.
        /// </summary>
        /// <param name="data">The data to load the name from.</param>
        /// <param name="logicalKitRoot">The logical tree node representing the root of the kit.</param>
        /// <returns>The kit name.</returns>
        internal static string GetKitName(ModuleData data, TreeNode logicalKitRoot)
        {
            var container = logicalKitRoot.Container;
            var (nameContainer, nameField) = container.ResolveField("KitCommon/KitName");
            var (subNameContainer, subNameField) = container.ResolveField("KitCommon/KitSubName");
            var name = data.GetDataField(nameContainer, nameField).FormattedText;
            var subName = data.GetDataField(subNameContainer, subNameField).FormattedText;
            return $"{name} / {subName}";
        }
    }
}
