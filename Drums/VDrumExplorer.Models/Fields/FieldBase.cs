using System;

namespace VDrumExplorer.Models.Fields
{
    public abstract class FieldBase
    {
        public string Description { get; }
        public string Path { get; }
        public int Address { get; }
        public int Size { get; }

        protected FieldBase(string description, string path, int address, int size) =>
            (Description, Path, Address, Size) = (description, path, address, size);

        // Note: we need to handle overflow from 0x7f to 0x100.
        protected int GetRawValue(ModuleData data) => throw new NotImplementedException();
    }
}
