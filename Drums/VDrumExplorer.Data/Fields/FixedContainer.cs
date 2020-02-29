// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;

namespace VDrumExplorer.Data.Fields
{
    /// <summary>
    /// A container fixed to a specific ModuleAddress.
    /// </summary>
    public sealed class FixedContainer
    {
        public Container Container { get; }
        public ModuleAddress Address { get; }

        public FixedContainer(Container container, ModuleAddress address) =>
            (Container, Address) = (container, address);

        public FixedContainer ToChildContext(Container container) =>
            new FixedContainer(container, Address + container.Offset);

        public IEnumerable<IPrimitiveField> GetPrimitiveFields(ModuleData data) =>
            GetChildren(data).OfType<IPrimitiveField>();

        /// <summary>
        /// Clones the loadable descendants of this container (including this container),
        /// loading the data into a new <see cref="ModuleData"/> at the given target address.
        /// </summary>
        public ModuleData CloneData(ModuleData data, ModuleAddress targetAddress)
        {
            int offset = targetAddress - Address;
            var clonedData = new ModuleData();
            var segments = AnnotateDescendantsAndSelf()
                .Select(ac => ac.Context)
                .Where(fc => fc.Container.Loadable)
                .Select(fc => data.GetSegment(fc.Address));
            foreach (var segment in segments)
            {
                clonedData.Populate(segment.Start + offset, segment.CopyData());
            }
            return clonedData;
        }

        public IEnumerable<IField> GetChildren(ModuleData data)
        {
            foreach (var field in Container.Fields)
            {
                if (field is DynamicOverlay overlay)
                {
                    var fields = overlay.GetOverlaidContainer(this, data).Fields;
                    foreach (var overlaidField in fields)
                    {
                        yield return overlaidField;
                    }
                }
                else
                {
                    yield return field;
                }
            }
        }

        public IEnumerable<AnnotatedContainer> AnnotateDescendantsAndSelf()
        {
            var queue = new Queue<AnnotatedContainer>();
            queue.Enqueue(new AnnotatedContainer("", this));
            while (queue.Count != 0)
            {
                var current = queue.Dequeue();
                yield return current;
                foreach (var container in current.Container.Fields.OfType<Container>())
                {
                    queue.Enqueue(current.AnnotateChildContainer(container));
                }
            }
        }

        public ValidationResult ValidateDescendantsAndSelf(ModuleData data)
        {
            int count = 0;
            var errors = new List<ValidationError>();
            foreach (var annotatedContainer in AnnotateDescendantsAndSelf())
            {
                var context = annotatedContainer.Context;
                foreach (var field in context.GetPrimitiveFields(data))
                {
                    count++;
                    if (!field.Validate(context, data, out var message))
                    {
                        string path = $"{annotatedContainer.Path}/{field.Name}";
                        var address = context.Address + field.Offset;
                        errors.Add(new ValidationError(path, address, field, message));
                    }
                }
            }
            return new ValidationResult(count, errors.AsReadOnly());
        }

        public IEnumerable<FixedContainer> DescendantsAndSelf()
        {
            var queue = new Queue<FixedContainer>();
            queue.Enqueue(this);
            while (queue.Count != 0)
            {
                var current = queue.Dequeue();
                yield return current;
                foreach (var container in current.Container.Fields.OfType<Container>())
                {
                    queue.Enqueue(current.ToChildContext(container));
                }
            }
        }

        public override string ToString() => Container.Description;
    }
}
