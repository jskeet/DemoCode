using System;

namespace VDrumExplorer.Data.Fields
{
    public abstract class FieldBase : IField
    {
        public string Description { get; }
        public FieldPath Path { get; }
        public ModuleAddress Address { get; }
        public int Size { get; }
        public FieldCondition? Condition { get; }

        protected FieldBase(FieldPath path, ModuleAddress address, int size, string description, FieldCondition? condition) =>
            (Path, Address, Size, Description, Condition) = (path, address, size, description, condition);

        public override string ToString() => Description;
    }
}
