// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Model.Data.Logical
{
    public class ListDataNodeDetail : IDataNodeDetail
    {
        /// <summary>
        /// The description of the list.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The items in the list.
        /// </summary>
        public IReadOnlyList<DataFieldFormattableString> Items { get; }

        public ListDataNodeDetail(string description, IReadOnlyList<DataFieldFormattableString> items) =>
            (Description, Items) = (description, items);
    }
}
