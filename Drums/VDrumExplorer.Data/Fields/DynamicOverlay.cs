using System;
using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public sealed class DynamicOverlay : FieldBase, IContainerField
    {
        private readonly ModuleAddress switchAddress;
        private readonly string switchTransform;
        private readonly IReadOnlyList<Container> containers;

        public DynamicOverlay(string description, string path, ModuleAddress address, int size,
            ModuleAddress switchAddress, string switchTransform, IReadOnlyList<Container> containers)
            : base(description, path, address, size) =>
            (this.switchAddress, this.switchTransform, this.containers) = (switchAddress, switchTransform, containers);

        public IEnumerable<IField> Children(ModuleData data)
        {
            var field = data.ModuleFields.PrimitiveFieldsByAddress[switchAddress];
            int index = switchTransform switch
            {
                null => ((FieldBase) field).GetRawValue(data),
                "instrumentGroup" => ((InstrumentField) field).GetInstrument(data).Group.Index,
                _ => throw new InvalidOperationException($"Invalid switch transform '{switchTransform}'")
            };
            return containers[index].Fields;
        }
    }
}
