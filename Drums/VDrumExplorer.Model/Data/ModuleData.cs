// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
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

        internal IEnumerable<(string, string)> GetDataFieldFormattedValues(FieldContainer container)
        {
            List<(string, string)> pairs = new();
            foreach (var field in GetDataFields(container))
            {
                AddField(field);
            }
            return pairs;

            void AddField(IDataField field, string namePrefix = "")
            {
                if (field is OverlayDataField odf)
                {
                    string prefix = field.SchemaField.Name + ".";
                    foreach (var overlayField in odf.CurrentFieldList.Fields)
                    {
                        AddField(overlayField, prefix);
                    }
                }
                else
                {
                    pairs.Add((namePrefix + field.SchemaField.Name, field.FormattedText));
                }
            }
        }

        // TODO: Put this into FieldContainer accepting a list of key/value pairs? (And a similar reverse operation?)
        // The traversal down containers may be trickier to generalize.
        internal void MergeTextValues(FieldContainer container, IEnumerable<(string key, string value)> values, ILogger logger)
        {
            foreach (var (fieldName, value) in values)
            {
                var dataField = GetDataField(fieldName);
                if (dataField is null)
                {
                    logger.LogWarning("Field '{name}' not found in container '{container}'", fieldName, container.Path);
                    continue;
                }
                if (!dataField.TrySetFormattedText(value))
                {
                    logger.LogWarning("Invalid value for field '{field}': '{value}'", dataField.SchemaField.Path, value);
                }

            }

            IDataField? GetDataField(string name)
            {
                var field = container.GetFieldOrNull(name);
                if (field is not null)
                {
                    return this.GetDataField(field);
                }
                return GetOverlayDataField(name);
            }

            IDataField? GetOverlayDataField(string name)
            {
                int dotIndex = name.IndexOf('.');
                if (dotIndex == -1)
                {
                    return null;
                }
                string beforeDot = name.Substring(0, dotIndex);
                var overlay = container.GetFieldOrNull(beforeDot) as OverlayField;
                if (overlay is null)
                {
                    return null;
                }
                string afterDot = name.Substring(dotIndex + 1);
                // We assume the controlling type has been set before the overlay fields appear...
                var overlayFields = ((OverlayDataField) this.GetDataField(overlay)).CurrentFieldList;
                return overlayFields.Fields.FirstOrDefault(f => f.SchemaField.Name == afterDot);
            }
        }

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

        public ModuleDataSnapshot CreatePartialSnapshot(TreeNode root)
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
        /// in this object. This is somewhat dangerous - it should only be called with a snapshot
        /// which contains coherent data for the target, e.g. a whole kit, or a whole trigger etc.
        /// </summary>
        public void LoadPartialSnapshot(ModuleDataSnapshot snapshot, ILogger logger)
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
                        var errors = field.Load(segment);
                        foreach (var error in errors)
                        {
                            // Just treat data validation errors as warnings; they're not *program* errors.
                            logger.LogWarning(error.ToString());
                        }
                    }
                }
            }
        }

        public void LoadSnapshot(ModuleDataSnapshot snapshot, ILogger logger)
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

            LoadPartialSnapshot(snapshot, logger);
        }

        /// <summary>
        /// Converts this instance to a new <see cref="ModuleData"/> to the given schema,
        /// using the formatted text values (and field/cotnainer names) in the conversion,
        /// logging any conversion failures along the way. This is not likely to go well
        /// unless the new and old schema are very similar.
        /// </summary>
        public ModuleData ConvertToSchema(ModuleSchema schema, ILogger logger)
        {
            var root = schema.LogicalRoot.ResolveNode(LogicalRoot.SchemaNode.Path);
            var newData = new ModuleData(root);
            MergeData(LogicalRoot.SchemaNode.Container, root.Container);
            return newData;

            void MergeData(IContainer oldContainer, IContainer newContainer)
            {
                switch ((oldContainer, newContainer))
                {
                    case (FieldContainer ofc, FieldContainer nfc):
                        MergeFields(ofc, nfc);
                        break;
                    case (ContainerContainer occ, ContainerContainer ncc):
                        MergeContainers(occ, ncc);
                        break;
                    default:
                        logger.LogError("Schema conversion error: '{path}' container types are not the same", oldContainer.Path);
                        break;
                }

                // TODO: Put this into FieldContainer accepting a list of key/value pairs? (And a similar reverse operation?)
                // The traversal down containers may be trickier to generalize.
                void MergeFields(FieldContainer ofc, FieldContainer nfc)
                {
                    var pairs = this.GetDataFieldFormattedValues(ofc);
                    newData.MergeTextValues(nfc, pairs, logger);
                }

                void MergeContainers(ContainerContainer occ, ContainerContainer ncc)
                {
                    foreach (var oldChild in occ.Containers)
                    {
                        var newChild = ncc.GetContainerOrNull(oldChild.Name);
                        if (newChild is null)
                        {
                            logger.LogWarning("Ignoring container '{container}': no such container in target schema", oldChild.Path);
                            continue;
                        }
                        MergeData(oldChild, newChild);
                    }
                }
            }
        }
    }
}
