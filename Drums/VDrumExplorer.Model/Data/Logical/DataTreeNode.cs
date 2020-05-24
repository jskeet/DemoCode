// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Data.Logical
{
    public class DataTreeNode
    {
        internal ModuleData Data { get; }
        public IReadOnlyList<DataTreeNode> Children { get; }
        public IReadOnlyList<IDataNodeDetail> Details { get; }
        public DataFieldFormattableString Format { get; }
        public TreeNode SchemaNode { get; }

        public DataTreeNode(ModuleData data, TreeNode node)
        {
            Data = data;
            Children = node.Children.ToReadOnlyList(child => new DataTreeNode(Data, child));
            Details = node.Details.ToReadOnlyList(detail => ConvertDetail(Data, detail));
            Format = new DataFieldFormattableString(data, node.Format);
            SchemaNode = node;
        }

        private static IDataNodeDetail ConvertDetail(ModuleData data, INodeDetail detail) =>
            detail switch
            {
                FieldContainerNodeDetail fieldContainerDetail =>
                    new FieldContainerDataNodeDetail(fieldContainerDetail.Description, fieldContainerDetail.Container, data),
                ListNodeDetail listDetail =>
                    new ListDataNodeDetail(listDetail.Description, listDetail.Items.ToReadOnlyList(ffs => new DataFieldFormattableString(data, ffs))),
                _ => throw new ArgumentException("Don't know how to convert {detail} to {nameof(IDataNodeDetail)}")
            };

        public int? GetMidiNote()
        {
            if (SchemaNode.MidiNotePath is null)
            {
                return null;
            }
            var (container, field) = SchemaNode.Container.ResolveField(SchemaNode.MidiNotePath);
            var midiField = (NumericDataField) Data.GetDataField(container, field);
            var value = midiField.RawValue;
            // 128 means "off"
            return value >= 0 && value < 128 ? value : default(int?);
        }
    }
}
