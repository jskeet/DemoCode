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

        private ContainerBase Root => Parent?.Root ?? this;

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
        public IContainer ResolveContainer(string path) => ResolveContainer(path.AsSpan());

        private IContainer ResolveContainer(ReadOnlySpan<char> span)
        {
            if (span.Length == 0 || (span.Length == 1 && span[0] == '.'))
            {
                return this;
            }
            // StartsWith("../")
            if (span[0] == '.' && span.Length >= 3 && span[1] == '.' && span[2] == '/')
            {
                return Parent?.ResolveContainer(span.Slice(3)) ?? throw new ArgumentException("Can't use .. in path from root container");
            }
            if (span[0] == '/')
            {
                return Root.ResolveContainer(span.Slice(1));
            }

            IContainer current = this;
            int segmentStart = 0;
            for (int i = 0; i <= span.Length; i++)
            {
                if (i == span.Length || span[i] == '/')
                {
                    var segment = span.Slice(segmentStart, i - segmentStart);
                    if (current is ContainerContainer cc)
                    {
                        current = cc.GetContainerOrNull(segment)
                            ?? throw new ArgumentException($"Container '{segment.ToString()}' not found within container '{cc.Path}'");
                    }
                    else
                    {
                        throw new ArgumentException($"Path segment '{segment.ToString()}' in path '{span.ToString()}' not found, resolving against '{Path}'");
                    }
                    segmentStart = i + 1;
                }
            }
            return current;
        }

        /// <inheritdoc />
        public IField ResolveField(string path)
        {
            // First separate the path into "container" and "field".
            var span = path.AsSpan();
            int lastSlash = path.LastIndexOf('/');
            var fieldName = lastSlash == -1 ? span : span.Slice(lastSlash + 1);

            // Now find the container, and the field within it.
            IContainer container = lastSlash == -1 ? this : ResolveContainer(span.Slice(0, lastSlash));
            return container is FieldContainer fc
                ? fc.GetFieldOrNull(fieldName)?? throw new ArgumentException($"No field '{fieldName.ToString()}' within container '{fc.Path}'")
                : throw new ArgumentException($"Container '{container.Path}' is not a field container, so cannot contain field '{fieldName.ToString()}'");
        }
    }
}
