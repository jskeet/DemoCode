using System;

namespace VDrumExplorer.Models.Fields
{
    public abstract class FieldVisitor
    {
        public virtual void Visit(FieldBase field)
        {
            if (field is IContainerField container)
            {
                VisitIContainerField(container);
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

        public virtual void VisitIContainerField(IContainerField containerField)
        {
            if (containerField is Container container)
            {
                VisitContainer(container);
                foreach (var field in container.Fields)
                {
                    Visit(field);
                }
            }
            else if (containerField is DynamicOverlay overlay)
            {
                VisitDynamicOverlay(overlay);
                // TODO: Recurse over the overlaid containers?
            }
            else
            {
                throw new InvalidOperationException($"Every field should be a container or a dynamic overlay");
            }
        }

        public virtual void VisitDynamicOverlay(DynamicOverlay overlay)
        {
        }

        public virtual void VisitContainer(Container container)
        {
        }
    }
}
