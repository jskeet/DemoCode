// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Data.Logical;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Model.Schema.Physical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Data
{
    /// <summary>
    /// The data for a module.
    /// </summary>
    public sealed class ModuleData
    {
        /// <summary>
        /// The physical root container; all data is present within this root.
        /// (This may not be the module root, however.)
        /// </summary>
        public IContainer PhysicalRoot { get; }

        /// <summary>
        /// The logical root container; all data is present within this root.
        /// (This may not be the module root, however.)
        /// </summary>
        public DataTreeNode LogicalRoot { get; }

        private readonly IReadOnlyDictionary<ModuleAddress, FieldContainerData> containers = new Dictionary<ModuleAddress, FieldContainerData>();

        private ModuleData(TreeNode logicalRoot, IEnumerable<(FieldContainer, byte[])> mappedData)
        {
            PhysicalRoot = logicalRoot.Container;
            containers = mappedData
                .ToDictionary(pair => pair.Item1.Address, pair => new FieldContainerData(this, pair.Item1, pair.Item2))
                .AsReadOnly();
            LogicalRoot = new DataTreeNode(this, logicalRoot);
        }

        internal static ModuleData FromData(TreeNode logicalRoot, IDictionary<ModuleAddress, byte[]> data)
        {
            var physicalRoot = (ContainerContainer) logicalRoot.Container;
            var containers = physicalRoot.DescendantsAndSelf().OfType<FieldContainer>().ToList();
            if (data.Count != containers.Count)
            {
                throw new ArgumentException($"Expected {containers.Count} data segments; received {data.Count}");
            }
            var mappedData = new List<(FieldContainer, byte[])>();
            foreach (var container in containers)            
            {
                if (!data.TryGetValue(container.Address, out var segment))
                {
                    throw new ArgumentException($"Data does not contain segment for address {container.Address}");
                }
                mappedData.Add((container, segment));
            }
            return new ModuleData(logicalRoot, mappedData);
        }

        public IDataField CreateDataField(FieldContainer container, IField field)
        {
            var data = containers[container.Address];
            return field switch
            {
                StringField f => new StringDataField(data, f),
                BooleanField f => new BooleanDataField(data, f),
                NumericField f => new NumericDataField(data, f),
                EnumField f => new EnumDataField(data, f),
                InstrumentField f => new InstrumentDataField(data, f),
                OverlayField f => new OverlayDataField(data, f),
                _ => throw new ArgumentException($"Can't handle {field} yet")
            };
        }

        public IEnumerable<FieldContainerData> Containers => containers.Values.OrderBy(c => c.FieldContainer.Address);

        public FieldContainerData GetContainerData(FieldContainer container) => containers[container.Address];
    }
}
