// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            
        private ModuleData(TreeNode logicalSchemaRoot)
        {
            PhysicalRoot = logicalSchemaRoot.Container;
            var schema = PhysicalRoot.Schema;
            var fieldContainers = schema.PhysicalRoot.DescendantsAndSelf().OfType<FieldContainer>().ToList();

            // First populate the containers
            var fieldMap = new SortedDictionary<FieldContainer, IReadOnlyList<IDataField>>(FieldContainer.AddressComparer);
            foreach (var fieldContainer in fieldContainers)
            {
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

            // Reset all fields to default values.
            foreach (var pair in fieldsByFieldContainer)
            {
                foreach (var field in pair.Value)
                {
                    field.Reset();
                }
            }

            // Note: must populate this *after* containers.
            LogicalRoot = new DataTreeNode(this, logicalSchemaRoot);
        }

        internal IReadOnlyList<IDataField> GetDataFields(FieldContainer container) => fieldsByFieldContainer[container];

        // TODO: Check how often this is called, and whether we need to optimize.
        internal IDataField GetDataField(FieldContainer container, IField field) =>
            GetDataFields(container).FirstOrDefault(dataField => dataField.SchemaField == field);

        /// <summary>
        /// Creates a new instance with default values for all fields.
        /// </summary>
        /// <param name="root">The root of the data. (Typically the module root or a kit root.)</param>
        public static ModuleData FromLogicalRootNode(TreeNode root) => new ModuleData(root);

        public ModuleDataSnapshot CreateSnapshot()
        {
            var snapshot = new ModuleDataSnapshot();
            foreach (var (fieldContainer, fieldList) in fieldsByFieldContainer)
            {
                var segment = new DataSegment(fieldContainer.Address, new byte[fieldContainer.Size]);
                foreach (DataFieldBase field in fieldList)
                {
                    field.Save(segment);
                }
                snapshot.Add(segment);
            }
            return snapshot;
        }

        public void LoadSnapshot(ModuleDataSnapshot snapshot)
        {
            if (snapshot.SegmentCount != fieldsByFieldContainer.Count)
            {
                throw new ArgumentException($"Expected {fieldsByFieldContainer.Count} data segments; snapshot contains {snapshot.SegmentCount}");
            }

            // Validate we have all the segments before we start mutating the fields.
            foreach (var container in fieldsByFieldContainer.Keys)
            {
                if (!snapshot.TryGetSegment(container.Address, out var segment))
                {
                    throw new ArgumentException($"Data does not contain segment for address {container.Address}");
                }
            }

            foreach (var (fieldContainer, fieldList) in fieldsByFieldContainer)
            {
                var segment = snapshot[fieldContainer.Address];
                foreach (DataFieldBase field in fieldList)
                {
                    field.Load(segment);
                }
            }
        }
    }
}
