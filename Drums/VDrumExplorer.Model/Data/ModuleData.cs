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
        /// The logical root container; all data is present within this root.
        /// (This may not be the module root, however.)
        /// </summary>
        public DataTreeNode LogicalRoot { get; }

        public ModuleSchema Schema => LogicalRoot.SchemaNode.Container.Schema;

        private readonly IReadOnlyDictionary<FieldContainer, IReadOnlyList<IDataField>> fieldsByFieldContainer;

        /// <summary>
        /// Originally-loaded data segments, potentially containing data not associated
        /// with any known field. (This helps for schemas where not all fields are fully
        /// understood, e.g. the AE-10.)
        /// </summary>
        private readonly Dictionary<FieldContainer, DataSegment> originalSegmentsByFieldContainer;
            
        private ModuleData(TreeNode logicalSchemaRoot)
        {
            var schema = logicalSchemaRoot.Container.Schema;
            var fieldContainers = logicalSchemaRoot.DescendantFieldContainers().ToList();
            originalSegmentsByFieldContainer = new Dictionary<FieldContainer, DataSegment>();

            // First populate the containers
            var fieldMap = new SortedDictionary<FieldContainer, IReadOnlyList<IDataField>>(FieldContainer.AddressComparer);
            foreach (var fieldContainer in fieldContainers)
            {
                fieldMap[fieldContainer] = fieldContainer.Fields.ToReadOnlyList(field => DataFieldBase.CreateDataField(field, schema));
                originalSegmentsByFieldContainer[fieldContainer] = new DataSegment(fieldContainer.Address, new byte[fieldContainer.Size]);
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
        internal IDataField GetDataField(IField field) =>
            GetDataFields(field.Parent ?? throw new ArgumentException($"Only parented fields can have data fields"))
                .FirstOrDefault(dataField => dataField.SchemaField == field);

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
                snapshot.Add(CreateDataSegment(fieldContainer, fieldList));
            }
            return snapshot;
        }

        internal ModuleDataSnapshot CreatePartialSnapshot(TreeNode root)
        {
            if (root.Container.Schema != Schema)
            {
                throw new ArgumentException("Invalid root for snapshot: incorrect schema");
            }

            var fieldContainers = root.DescendantFieldContainers().ToList();
            var snapshot = new ModuleDataSnapshot();
            foreach (var fc in fieldContainers)
            {
                snapshot.Add(CreateDataSegment(fc, fieldsByFieldContainer[fc]));
            }
            return snapshot;
        }

        private DataSegment CreateDataSegment(FieldContainer fieldContainer, IReadOnlyList<IDataField> fields)
        {
            var segment = originalSegmentsByFieldContainer[fieldContainer].Clone();
            foreach (DataFieldBase field in fields)
            {
                field.Save(segment);
            }
            return segment;
        }

        /// <summary>
        /// Loads data from a snapshot which could contain just part of the data contained
        /// in this object. This is internal as it's inherently somewhat dangerous - it should only be called
        /// for well-defined snapshots, e.g. "a whole kit".
        /// </summary>
        internal void LoadPartialSnapshot(ModuleDataSnapshot snapshot)
        {
            // TODO: This is more awkward than it should be, because we keep a map based on field containers rather than
            // addresses. We could change the map... then we might not need the FieldContainer.AddressComparer, either.
            foreach (var (container, fieldList) in fieldsByFieldContainer)
            {
                if (snapshot.TryGetSegment(container.Address, out var segment))
                {
                    originalSegmentsByFieldContainer[container] = segment.Clone();
                    foreach (DataFieldBase field in fieldList)
                    {
                        field.Load(segment);
                    }
                }
            }
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
                originalSegmentsByFieldContainer[fieldContainer] = segment.Clone();
                foreach (DataFieldBase field in fieldList)
                {
                    field.Load(segment);
                }
            }
        }
    }
}
