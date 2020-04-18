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

        private readonly IReadOnlyDictionary<FieldContainer, IReadOnlyList<IDataField>> fieldsByFieldContainer;
            
        private ModuleData(TreeNode logicalSchemaRoot, Dictionary<ModuleAddress, DataSegment> mappedSegments)
        {
            PhysicalRoot = logicalSchemaRoot.Container;
            var schema = PhysicalRoot.Schema;
            var fieldContainers = schema.PhysicalRoot.DescendantsAndSelf().OfType<FieldContainer>().ToList();
            if (mappedSegments.Count != fieldContainers.Count)
            {
                throw new ArgumentException($"Expected {fieldContainers.Count} data segments; received {mappedSegments.Count}");
            }

            // First populate the containers
            var fieldMap = new SortedDictionary<FieldContainer, IReadOnlyList<IDataField>>(FieldContainer.AddressComparer);
            foreach (var fieldContainer in fieldContainers)
            {
                if (!mappedSegments.TryGetValue(fieldContainer.Address, out var segment))
                {
                    throw new ArgumentException($"Data does not contain segment for address {fieldContainer.Address}");
                }
                fieldMap[fieldContainer] = fieldContainer.Fields.ToReadOnlyList(field => DataFieldBase.CreateDataField(field, schema));
            }
            fieldsByFieldContainer = fieldMap;

            IEnumerable<DataFieldBase> allFields = fieldsByFieldContainer.SelectMany(pair => pair.Value).Cast<DataFieldBase>();

            // Now resolve fields
            foreach (var pair in fieldsByFieldContainer)
            {
                foreach (var field in pair.Value.Cast<DataFieldBase>())
                {
                    field.ResolveFields(this, pair.Key);
                }
            }
            // Now load the data
            foreach (var (fieldContainer, fieldList) in fieldsByFieldContainer)
            {
                if (!mappedSegments.TryGetValue(fieldContainer.Address, out var segment))
                {
                    throw new ArgumentException($"Data does not contain segment for address {fieldContainer.Address}");
                }
                foreach (DataFieldBase field in fieldList)
                {
                    field.Load(segment);
                }
            }

            // Note: must populate this *after* containers.
            LogicalRoot = new DataTreeNode(this, logicalSchemaRoot);
        }

        internal static ModuleData FromData(TreeNode logicalRoot, IEnumerable<DataSegment> dataSegments)
        {
            var mappedSegments = dataSegments.ToDictionary(segment => segment.Address);
            return new ModuleData(logicalRoot, mappedSegments);
        }

        internal IReadOnlyList<IDataField> GetDataFields(FieldContainer container) => fieldsByFieldContainer[container];

        // TODO: Check how often this is called, and whether we need to optimize.
        internal IDataField GetDataField(FieldContainer container, IField field) =>
            GetDataFields(container).FirstOrDefault(dataField => dataField.SchemaField == field);

        public IEnumerable<DataSegment> SerializeData()
        {
            foreach (var (fieldContainer, fieldList) in fieldsByFieldContainer)
            {
                var segment = new DataSegment(fieldContainer.Address, new byte[fieldContainer.Size]);
                foreach (DataFieldBase field in fieldList)
                {
                    field.Save(segment);
                }
                yield return segment;
            }
        }
    }
}
