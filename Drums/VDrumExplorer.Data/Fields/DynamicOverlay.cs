using System;
using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public sealed class DynamicOverlay : FieldBase, IContainerField
    {
        private readonly ModuleAddress switchAddress;
        private readonly string? switchTransform;
        internal IReadOnlyList<Container> OverlaidContainers { get; }
        
        // No condition; overlays are already conditional effectively.
        public DynamicOverlay(FieldPath path, ModuleAddress address, int size, string description,
            ModuleAddress switchAddress, string? switchTransform, IReadOnlyList<Container> containers)
            : base(path, address, size, description, null) =>
            (this.switchAddress, this.switchTransform, OverlaidContainers) = (switchAddress, switchTransform, containers);

        public IEnumerable<IField> Children(ModuleData data)
        {
            var field = data.Schema.PrimitiveFieldsByAddress[switchAddress];
            int index = switchTransform switch
            {
                null => ((NumericFieldBase) field).GetRawValue(data),
                "instrumentGroup" => ((InstrumentField) field).GetInstrument(data).Group.Index,
                _ => throw new InvalidOperationException($"Invalid switch transform '{switchTransform}'")
            };
            return OverlaidContainers[index].Fields;
        }
    }
}
