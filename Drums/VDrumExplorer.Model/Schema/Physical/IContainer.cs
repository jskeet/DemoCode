// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using VDrumExplorer.Model.Schema.Fields;

namespace VDrumExplorer.Model.Schema.Physical
{
    /// <summary>
    /// Common interface for all containers.
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// The name of the container. This forms the last part of the path.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The description of the container.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Absolute path to this container within the module.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The address of this container within the module data.
        /// </summary>
        ModuleAddress Address { get; }

        /// <summary>
        /// The parent of this container, or null if this is the root container.
        /// </summary>
        ContainerContainer? Parent { get; }

        ModuleSchema Schema { get; }

        /// <summary>
        /// Resolves a container with the specified path, which may be relative or absolute.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <returns>The container at the given path.</returns>
        IContainer ResolveContainer(string path);

        /// <summary>
        /// Resolves a field with the specified path, which may be relative or absolute.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <returns>The field and its parent field container.</returns>
        (FieldContainer container, IField field) ResolveField(string path);
    }
}
