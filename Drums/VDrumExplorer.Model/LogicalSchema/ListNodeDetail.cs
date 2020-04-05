// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Model.LogicalSchema
{
    /// <summary>
    /// A list of formattted details for a tree node.
    /// </summary>
    public sealed class ListNodeDetail : INodeDetail
    {
        /// <summary>
        /// The description of the list.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The items in the list.
        /// </summary>
        public IReadOnlyList<FieldFormattableString> Items { get; }

        public ListNodeDetail(string description, IReadOnlyList<FieldFormattableString> items) =>
            (Description, Items) = (description, items);

        public override string ToString() => $"{Description}: {Items.Count} fields";
    }
}
