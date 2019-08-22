// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// A data container representing a portion of memory.
    /// </summary>
    public sealed class Container : FieldBase, IContainerField
    {
        public IReadOnlyList<IField> Fields { get; }
        internal IReadOnlyDictionary<ModuleAddress, IField> FieldsByAddress { get; }
        public bool Loadable { get; }
        
        public string? Name { get; }

        internal Container(FieldBase.Parameters common, string? name, IReadOnlyList<IField> fields)
            : base(common)
        {
            Name = name;
            Fields = fields;
            Loadable = !Fields.Any(f => f is Container);
            FieldsByAddress = Fields.ToDictionary(f => f.Address).AsReadOnly();
        }

        public IEnumerable<IField> Children(ModuleData data) => Fields;

        /// <summary>
        /// Returns all fields in this container recursively. Dynamic overlay fields are returned as they are,
        /// with no further resolution.
        /// </summary>
        /// <returns>A sequence of fields, including the container itself.</returns>
        public IEnumerable<IField> DescendantsAndSelf()
        {
            yield return this;
            foreach (var field in Fields)
            {
                if (field is Container container)
                {
                    foreach (var descendant in container.DescendantsAndSelf())
                    {
                        yield return descendant;
                    }
                }
                else
                {
                    yield return field;
                }
            }
        }

        /// <summary>
        /// Returns all fields in this container recursively, in a breadth-first manner. Dynamic overlay fields are processed
        /// according to the data in <paramref name="data"/>; the field itself is not returned,
        /// but the overlaid fields are, having resolved the relevant container.
        /// </summary>
        /// <returns>A sequence of fields, including the container itself.</returns>
        public IEnumerable<IField> DescendantsAndSelf(ModuleData data)
        {
            Queue<Container> containerQueue = new Queue<Container>();
            containerQueue.Enqueue(this);
            while (containerQueue.Count > 0)
            {
                var container = containerQueue.Dequeue();
                yield return container;
                foreach (var field in container.Fields)
                {
                    if (field is IPrimitiveField primitive)
                    {
                        yield return primitive;
                    }
                    else if (field is DynamicOverlay overlay)
                    {
                        // We assume overlays are shallow. If they're not, we'll find out via an exception in the cast...
                        // (We also assume the field the overlay switches on is present... maybe we shouldn't.)
                        var overlaid = overlay.GetOverlaidContainer(data);
                        foreach (var overlaidPrimitive in overlaid.Fields)
                        {
                            yield return (IPrimitiveField) overlaidPrimitive;
                        }
                    }
                    else if (field is Container subcontainer)
                    {
                        containerQueue.Enqueue(subcontainer);
                    }
                }
            }
        }

        /// <summary>
        /// Resets all primitive fields in the container to valid values, recursively.
        /// (For the moment, we'll assume that's all that's required.)
        /// </summary>
        public void Reset(ModuleData data)
        {
            foreach (var field in Fields.OfType<IPrimitiveField>())
            {
                field.Reset(data);
            }
        }
    }
}
