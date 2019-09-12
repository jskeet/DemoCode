﻿// Copyright 2019 Jon Skeet. All rights reserved.
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
    }
}
