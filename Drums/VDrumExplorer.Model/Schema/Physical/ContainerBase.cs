// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using VDrumExplorer.Model.Schema.Fields;
using VDrumExplorer.Utility;

namespace VDrumExplorer.Model.Schema.Physical
{
    /// <summary>
    /// Base class for containers.
    /// </summary>
    public abstract class ContainerBase : IContainer
    {
        /// <inheritdoc />
        public string Name { get; }
        /// <inheritdoc />
        public string Description { get; }
        /// <inheritdoc />
        public ModuleAddress Address { get; }
        /// <inheritdoc />
        public string Path { get; }

        /// <inheritdoc />
        public ContainerContainer? Parent { get; internal set; }

        /// <inheritdoc />
        public ModuleSchema Schema { get; }

        private protected ContainerBase(ModuleSchema schema, string name, string description, ModuleAddress address, string path) =>
            (Schema, Name, Description, Address, Path) = (schema, name, description, address, path);

        /// <inheritdoc />
        public override string ToString() => Description;

        private IContainer Root => Parent?.Root ?? this;

        // Note: this method knows that it only needs to recurse for ContainerContainer, which is a little odd, but simplifies the code.
        public IEnumerable<IContainer> DescendantsAndSelf()
        {
            Queue<IContainer> queue = new Queue<IContainer>();
            queue.Enqueue(this);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return current;
                if (current is ContainerContainer cc)
                {
                    foreach (var item in cc.Containers)
                    {
                        queue.Enqueue(item);
                    }
                }
            }
        }

        /// <inheritdoc />
        public IContainer ResolveContainer(string path)
        {
            if (path == "." || path == "")
            {
                return this;
            }
            if (path.StartsWith("../"))
            {
                return Parent?.ResolveContainer(path.Substring(3)) ?? throw new ArgumentException("Can't use .. in path from root container");
            }
            if (path.StartsWith("/"))
            {
                return Root.ResolveContainer(path.Substring(1));
            }
            var segments = path.Split('/');
            IContainer current = this;
            foreach (var segment in segments)
            {
                if (current is ContainerContainer cc)
                {
                    current = cc.ContainersByName.GetValueOrDefault(segment)
                        ?? throw new ArgumentException($"Container '{segment}' not found within container '{cc.Path}'");
                }
                else
                {
                    throw new ArgumentException($"Path segment '{segment}' in path '{path}' not found, resolving against '{Path}'");
                }
            }
            return current;
        }

        /// <inheritdoc />
        public (FieldContainer container, IField field) ResolveField(string path)
        {
            // First separate the path into "container" and "field".
            int lastSlash = path.LastIndexOf('/');
            string containerPath = lastSlash == -1 ? "." : path.Substring(0, lastSlash);
            string fieldName = lastSlash == -1 ? path : path.Substring(lastSlash + 1);

            // Now find the container, and the field within it.
            IContainer container = ResolveContainer(containerPath);
            return container is FieldContainer fc
                ? (fc, fc.FieldsByName.GetValueOrDefault(fieldName) ?? throw new ArgumentException($"No field '{fieldName}' within container '{fc.Path}'"))
                : throw new ArgumentException($"Container '{container.Path}' is not a field container, so cannot contain field '{fieldName}'");
        }
    }
}
