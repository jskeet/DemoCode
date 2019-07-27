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

        internal virtual int GetRawValue(ModuleData data) =>
            Size switch
            {
                1 => data.GetAddressValue(Address),
                2 => (int)(sbyte)((data.GetAddressValue(Address) << 4) | data.GetAddressValue(Address + 1)),
                // TODO: Just fetch a byte array? Stackalloc it?
                4 => (short)(
                    (data.GetAddressValue(Address) << 12) |
                    (data.GetAddressValue(Address + 1) << 8) |
                    (data.GetAddressValue(Address + 2) << 4) |
                    data.GetAddressValue(Address + 3)),
                _ => throw new InvalidOperationException($"Cannot get value with size {Size}")
            };
        public override string ToString() => Description;
    }
}
