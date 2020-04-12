// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using VDrumExplorer.Model.Schema.Logical;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Data.Logical
{
    public class DataTreeNode
    {
        public IReadOnlyList<DataTreeNode> Children { get; }
        public IReadOnlyList<IDataNodeDetail> Details { get; }
        public DataFieldFormattableString Format { get; }

        public DataTreeNode(ModuleData data, TreeNode node)
        {
            Children = node.Children.ToReadOnlyList(child => new DataTreeNode(data, child));
            Details = node.Details.ToReadOnlyList(detail => ConvertDetail(data, detail));
            Format = new DataFieldFormattableString(data, node.Format);
        }

        private static IDataNodeDetail ConvertDetail(ModuleData data, INodeDetail detail) =>
            detail switch
            {
                FieldContainerNodeDetail fieldContainerDetail =>
                    new FieldContainerDataNodeDetail(fieldContainerDetail.Description, data.GetContainerData(fieldContainerDetail.Container)),
                ListNodeDetail listDetail =>
                    new ListDataNodeDetail(listDetail.Description, listDetail.Items.ToReadOnlyList(ffs => new DataFieldFormattableString(data, ffs))),
                _ => throw new ArgumentException("Don't know how to convert {detail} to {nameof(IDataNodeDetail)}")
            };
    }
}
