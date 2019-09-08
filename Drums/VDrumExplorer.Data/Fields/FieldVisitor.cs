// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace VDrumExplorer.Data.Fields
{
    public abstract class FieldVisitor
    {
        public virtual void Visit(IField field)
        {
            if (field is DynamicOverlay overlay)
            {
                VisitDynamicOverlay(overlay);
            }
            if (field is Container container)
            {
                VisitContainer(container);
            }
            else if (field is IPrimitiveField primitive)
            {
                VisitIPrimitiveField(primitive);
            }
            else
            {
                throw new InvalidOperationException($"Every field should be a primitive or a container field");
            }
        }

        public virtual void VisitIPrimitiveField(IPrimitiveField primitiveField)
        {
        }

        public virtual void VisitDynamicOverlay(DynamicOverlay overlay)
        {
            // TODO: Recurse over the overlaid containers?
        }

        public virtual void VisitContainer(Container container)
        {
            foreach (var field in container.Fields)
            {
                Visit(field);
            }
        }
    }
}
