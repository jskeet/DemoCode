// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.PhysicalSchema;

namespace VDrumExplorer.Model.LogicalSchema
{
    /// <summary>
    /// A group of details for a tree node, where
    /// mapping to a field container in the physical schema.
    /// </summary>
    public sealed class FieldContainerNodeDetail : INodeDetail
    {
        /// <summary>
        /// The description of the fields being described.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The field container in the physical schema.
        /// </summary>
        public FieldContainer Container { get; }

        public FieldContainerNodeDetail(string description, FieldContainer container) =>
            (Description, Container) = (description, container);

        public override string ToString() => $"{Description}: {Container.Path}";
    }
}
