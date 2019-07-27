using System;

namespace VDrumExplorer.Data.Fields
{
    public abstract class FieldBase : IField
    {
        public string Description { get; }
        public FieldPath Path { get; }
        public ModuleAddress Address { get; }
        public int Size { get; }

        protected FieldBase(FieldPath path, ModuleAddress address, int size, string description) =>
            (Path, Address, Size, Description) = (path, address, size, description);

        public override string ToString() => Description;
    }
}
