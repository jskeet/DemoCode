// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Model.Schema.Physical;

namespace VDrumExplorer.Model.Data.Logical
{
    public sealed class FieldContainerDataNodeDetail : IDataNodeDetail
    {
        /// <summary>
        /// The description of the fields being described.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The field container and data in the physical schema.
        /// </summary>
        public FieldContainer Container { get; }

        public IReadOnlyList<IDataField> Fields { get; }

        public FieldContainerDataNodeDetail(string description, FieldContainer container, ModuleData data)
        {
            Description = description;
            Container = container;
            Fields = data.GetDataFields(container);
        }
    }
}
