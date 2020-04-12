// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using VDrumExplorer.Model.Data.Fields;
using VDrumExplorer.Utility;

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
        public FieldContainerData Container { get; }

        private readonly Lazy<IReadOnlyList<IDataField>> fields;
        public IReadOnlyList<IDataField> Fields => fields.Value;

        public FieldContainerDataNodeDetail(string description, FieldContainerData container)
        {
            Description = description;
            Container = container;
            fields = Lazy.Create(() => container.FieldContainer.Fields.ToReadOnlyList(field => container.ModuleData.CreateDataField(container.FieldContainer, field)));
        }
    }
}
