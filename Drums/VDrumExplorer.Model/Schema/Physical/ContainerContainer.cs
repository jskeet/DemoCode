// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Schema.Physical
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

        /// <summary>
        /// A map from container name to container.
        /// </summary>
        private IReadOnlyDictionary<string, IContainer> ContainersByName { get; }

        internal IContainer? GetContainerOrNull(string name) =>
            ContainersByName.GetValueOrDefault(name);

        internal ContainerContainer(ModuleSchema schema, string name, string description, ModuleAddress address, string path, List<IContainer> containers)
            : base(schema, name, description, address, path)
        {
            Containers = containers;
            foreach (ContainerBase container in Containers)
            {
                container.Parent = this;
            }
            ContainersByName = Containers.ToDictionary(c => c.Name).AsReadOnly();
        }
    }
}
