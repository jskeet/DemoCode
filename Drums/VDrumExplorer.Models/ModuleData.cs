using System;

namespace VDrumExplorer.Models
{
    public class ModuleData
    {
        public ModuleFields ModuleFields { get; }

        public ModuleData(ModuleFields moduleFields) =>
            ModuleFields = moduleFields;

        public byte GetAddressValue(int address)
        {
            if ((address & 0x80_80_80_80L) != 0)
            {
                throw new ArgumentException($"Invalid address: {address:x}. Top bit must not be set in any byte.");
            }
            throw new NotImplementedException();
        }
    }
}
