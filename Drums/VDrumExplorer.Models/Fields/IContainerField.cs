using System.Collections.Generic;

namespace VDrumExplorer.Models.Fields
{
    public interface IContainerField
    {
        IEnumerable<FieldBase> GetFields(ModuleData data);
    }
}
