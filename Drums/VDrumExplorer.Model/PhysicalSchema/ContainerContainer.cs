// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Model.PhysicalSchema
{
    /// <summary>
    /// A container that only contains other containers.
    /// </summary>
    public sealed class ContainerContainer : ContainerBase
    {
        /// <summary>
        /// The list of containers within this one.
        /// </summary>
        public IReadOnlyList<IContainer> Containers { get; }

        internal ContainerContainer(string name, string description, ModuleAddress address, string path, List<IContainer> containers)
            : base(name, description, address, path)
        {
            Containers = containers;
            foreach (ContainerBase container in Containers)
            {
                container.Parent = this;
            }
        }
    }
}
